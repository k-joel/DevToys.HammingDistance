using DevToys.Api;
using System.ComponentModel.Composition;
using System.Numerics;
using static DevToys.Api.GUI;

namespace DevToys.HammingDistance
{
    [Export(typeof(IGuiTool))]
    [Name("HammingDistance")]
    [ToolDisplayInformation(
        IconFontName = "FluentSystemIcons",
        IconGlyph = '\uf7f8',
        GroupName = PredefinedCommonToolGroupNames.Testers,
        ResourceManagerAssemblyIdentifier = nameof(HammingDistanceResourceAssemblyIdentifier),
        ResourceManagerBaseName = "DevToys.HammingDistance.Resource",
        ShortDisplayTitleResourceName = nameof(Resource.ShortDisplayTitle),
        LongDisplayTitleResourceName = nameof(Resource.LongDisplayTitle),
        DescriptionResourceName = nameof(Resource.Description),
        AccessibleNameResourceName = nameof(Resource.AccessibleName),
        SearchKeywordsResourceName = nameof(Resource.SearchKeywords))]
    internal sealed class HammingDistanceGui : IGuiTool
    {
        private readonly IUIMultiLineTextInput _input1 = MultiLineTextInput("input1");
        private readonly IUIMultiLineTextInput _input2 = MultiLineTextInput("input2");

        private readonly IUISingleLineTextInput _output = SingleLineTextInput("output");


        enum InputType
        {
            Binary,
            Octal,
            Decimal,
            Hexadecimal,
            Text
        }

        private InputType _currInputType1 = InputType.Binary;
        private InputType _currInputType2 = InputType.Binary;
        private int _lastStringSet = -1;


        public UIToolView View => new(
            Stack()
                .Vertical()
                .LargeSpacing()
                .WithChildren(
                    _input1.AlwaysWrap()
                        .Title("String 1 *")
                        .Text(string.Empty)
                        .OnTextChanged(_ => OnCalculate())
                        .CommandBarExtraContent(
                            Stack().Horizontal().WithChildren(
                                Label().Text("Input Type: ").Style(UILabelStyle.Body),
                                SelectDropDownList("inputType1")
                                    .WithItems(
                                        Item(nameof(InputType.Binary), InputType.Binary),
                                        Item(nameof(InputType.Octal), InputType.Octal),
                                        Item(nameof(InputType.Decimal), InputType.Decimal),
                                        Item(nameof(InputType.Hexadecimal), InputType.Hexadecimal),
                                        Item(nameof(InputType.Text), InputType.Text))
                                    .Select(0)
                                    .OnItemSelected(OnInputType1Changed)
                                )
                            ),

                    _input2.AlwaysWrap()
                        .Title("String 2 *")
                        .Text(string.Empty)
                        .OnTextChanged(_ => OnCalculate())
                        .CommandBarExtraContent(
                            Stack().Horizontal().WithChildren(
                                Label().Text("Input Type: ").Style(UILabelStyle.Body),
                                SelectDropDownList("inputType2")
                                    .WithItems(
                                        Item(nameof(InputType.Binary), InputType.Binary),
                                        Item(nameof(InputType.Octal), InputType.Octal),
                                        Item(nameof(InputType.Decimal), InputType.Decimal),
                                        Item(nameof(InputType.Hexadecimal), InputType.Hexadecimal),
                                        Item(nameof(InputType.Text), InputType.Text))
                                    .Select(0)
                                    .OnItemSelected(OnInputType2Changed)
                                )
                        ),

                    // Output
                    _output.ReadOnly()
                        .Title("Hamming Distance")
                        .Text(string.Empty)
                )
        );

        public void OnDataReceived(string dataTypeName, object? parsedData)
        {
            if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string value)
            {
                _lastStringSet = (_lastStringSet + 1) % 2;

                IUIMultiLineTextInput[] inputs = [_input1, _input2];
                inputs[_lastStringSet].Text(value);
            }
        }

        private void OnInputType1Changed(IUIDropDownListItem? obj)
        {
            _currInputType1 = obj?.Value is InputType it1 ? it1 : InputType.Text;
            OnCalculate();
        }

        private void OnInputType2Changed(IUIDropDownListItem? obj)
        {
            _currInputType2 = obj?.Value is InputType it2 ? it2 : InputType.Text;
            OnCalculate();
        }

        private void OnCalculate()
        {
            if (_input1.Text == string.Empty || _input2.Text == string.Empty)
            {
                _output.Text("Waiting for Input...");
                return;
            }

            try
            {
                string bits1 = ToBitString(_input1.Text, _currInputType1);
                string bits2 = ToBitString(_input2.Text, _currInputType2);

                int distance = CalculateHammingDistance(bits1, bits2);
                _output.Text($"{distance}");
            }
            catch (Exception ex)
            {
                _output.Text($"Error: {ex.Message}");
            }
        }

        private static int CalculateHammingDistance(string bits1, string bits2)
        {
            int maxLen = Math.Max(bits1.Length, bits2.Length);
            bits1 = bits1.PadLeft(maxLen, '0');
            bits2 = bits2.PadLeft(maxLen, '0');

            return bits1.Zip(bits2, (a, b) => a != b ? 1 : 0).Sum();
        }

        private static string StripWhitespace(string input) =>
            new(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        private static string TextToBitString(string input) =>
            string.Concat(input.Select(c => Convert.ToString(c, 2).PadLeft(16, '0')));

        private static string OctalToBitString(string input) =>
            string.Concat(input.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 8), 2).PadLeft(3, '0')));

        private static string HexToBitString(string input) =>
            string.Concat(input.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

        private static string BinaryToBitString(string input) =>
            input.All(c => c is '0' or '1')
                ? input
                : throw new ArgumentException("Binary input must contain only 0s and 1s.");

        private static string BigIntToBitString(string input)
        {
            var n = BigInteger.Parse(input);
            if (n < 0) throw new ArgumentException("Negative decimal values are not supported.");
            if (n == 0) return "0";
            var bits = new System.Text.StringBuilder();
            while (n > 0) { bits.Insert(0, (char)('0' + (int)(n % 2))); n >>= 1; }
            return bits.ToString();
        }

        private static string ToBitString(string input, InputType type)
        {
            if (type != InputType.Text)
                input = StripWhitespace(input);

            return type switch
            {
                InputType.Binary => BinaryToBitString(input),
                InputType.Octal => OctalToBitString(input),
                InputType.Decimal => BigIntToBitString(input),
                InputType.Hexadecimal => HexToBitString(input),
                _ => TextToBitString(input),
            };
        }
    }
}


