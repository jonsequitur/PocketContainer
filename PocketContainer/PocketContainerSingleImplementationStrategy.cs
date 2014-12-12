// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// It has been imported using NuGet from the PocketContainer project (https://github.com/jonsequitur/PocketContainer). 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Linq;

namespace Pocket
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal static class PocketContainerSingleImplementationStrategy
    {
        public static PocketContainer IfOnlyOneImplementationUseIt(
            this PocketContainer container)
        {
            return container.AddStrategy(type =>
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    var implementations = Discover.ConcreteTypes()
                                                  .DerivedFrom(type)
                                                  .ToArray();

                    if (implementations.Count() == 1)
                    {
                        var implementation = implementations.Single();
                        return c => c.Resolve(implementation);
                    }
                }
                return null;
            });
        }
    }
}