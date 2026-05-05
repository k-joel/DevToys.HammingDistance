using DevToys.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevToys.HammingDistance
{
    [Export(typeof(IResourceAssemblyIdentifier))]
    [Name(nameof(HammingDistanceResourceAssemblyIdentifier))]
    internal sealed class HammingDistanceResourceAssemblyIdentifier : IResourceAssemblyIdentifier
    {
        public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
        {
            return ValueTask.FromResult(Array.Empty<FontDefinition>());
        }
    }
}
