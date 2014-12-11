// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// It has been imported using NuGet from the PocketContainer project (https://github.com/jonsequitur/PocketContainer). 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Linq;
using System.Web.Http.Dependencies;

namespace Pocket
{
#if !RecipesProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal static class PocketContainerDependencyResolverStrategy
    {
        public static PocketContainer IncludeDependencyResolver(
            this PocketContainer container,
            IDependencyResolver dependencyResolver)
        {
            return container.AddStrategy(type =>
            {
                if (dependencyResolver.GetService(type) != null)
                {
                    return c => dependencyResolver.GetService(type);
                }
                return null;
            });
        }
    }
}