using DevToys.Api;
using System.ComponentModel.Composition;
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

        private readonly IUISelectDropDownList _inputType1 = SelectDropDownList("inputType1");
        private readonly IUISelectDropDownList _inputType2 = SelectDropDownList("inputType2");

        private readonly IUIMultiLineTextInput _output = MultiLineTextInput("output");

        private int _lastStringSet = -1;

        enum InputType
        {
            Binary,
            Octal,
            Decimal,
            Hexadecimal,
            Text
        }

        public UIToolView View => new(
            Stack()
                .Vertical()
                .WithChildren(
                    // String 1
                    Stack().Horizontal().WithChildren(
                        Label().Style(UILabelStyle.BodyStrong).Text("String 1 *"),
                        InfoBar("string1Info").Informational().Description("The first input string"),
                        _inputType1
                            .WithItems(
                                Item(nameof(InputType.Binary), InputType.Binary),
                                Item(nameof(InputType.Octal), InputType.Octal),
                                Item(nameof(InputType.Decimal), InputType.Decimal),
                                Item(nameof(InputType.Hexadecimal), InputType.Hexadecimal),
                                Item(nameof(InputType.Text), InputType.Text))
                            .Select(0)
                    ),
                    _input1.AlwaysWrap().HideCommandBar()
                        .Text(string.Empty),

                    // String 2
                    Stack().Horizontal().WithChildren(
                        Label().Style(UILabelStyle.BodyStrong).Text("String 2 *"),
                        InfoBar("string2Info").Informational().Description("The second input string"),
                        _inputType2
                            .WithItems(
                                Item(nameof(InputType.Binary), InputType.Binary),
                                Item(nameof(InputType.Octal), InputType.Octal),
                                Item(nameof(InputType.Decimal), InputType.Decimal),
                                Item(nameof(InputType.Hexadecimal), InputType.Hexadecimal),
                                Item(nameof(InputType.Text), InputType.Text))
                            .Select(0)
                    ),
                    _input2.AlwaysWrap().HideCommandBar()
                        .Text(string.Empty),

                    // Output
                    Label().Style(UILabelStyle.BodyStrong).Text("Output"),
                    _output.AlwaysWrap().HideCommandBar().ReadOnly().Text(string.Empty),

                    // Buttons
                    Stack().Horizontal().WithChildren(
                        Button("calculateBtn")
                            .AccentAppearance()
                            .Icon("FluentSystemIcons", '\uF18D')
                            .Text("Calculate")
                            .OnClick(OnCalculateClicked)
                    )
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

        private void OnCalculateClicked()
        {
            try
            {
                var string1 = _input1.Text;
                var string2 = _input2.Text;

                var inputType1 = _inputType1.SelectedItem?.Value is InputType it1 ? it1 : InputType.Text;
                var inputType2 = _inputType2.SelectedItem?.Value is InputType it2 ? it2 : InputType.Text;

                string bits1 = ToBitString(string1, inputType1);
                string bits2 = ToBitString(string2, inputType2);

                int distance = CalculateHammingDistance(bits1, bits2);
                _output.Text($"Hamming Distance: {distance}");
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

        private static string ToBitString(string input, InputType type) => type switch
        {
            InputType.Binary => input.Replace(" ", ""),
            InputType.Octal => string.Concat(input.Replace(" ", "").Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 8), 2).PadLeft(3, '0'))),
            InputType.Decimal => Convert.ToString(long.Parse(input.Trim()), 2),
            InputType.Hexadecimal => string.Concat(input.Replace(" ", "").Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0'))),
            _ => string.Concat(input.Select(c => Convert.ToString(c, 2).PadLeft(8, '0'))),
        };
    }
}


