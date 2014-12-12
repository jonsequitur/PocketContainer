// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// It has been imported using NuGet from the PocketContainer project (https://github.com/jonsequitur/PocketContainer). 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pocket
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif

    /// <summary>
    /// Performs runtime type discovery, useful for convention-based configuration tasks.
    /// </summary>
    internal static class Discover
    {
        public static IEnumerable<Type> DerivedFrom(this IEnumerable<Type> types, Type type)
        {
            return types.Where(type.IsAssignableFrom);
        }

        public static IEnumerable<Type> ConcreteTypes(bool includeGlobalAssemblyCache = false)
        {
            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(a => !a.IsDynamic)
                            .Where(a => includeGlobalAssemblyCache || !a.GlobalAssemblyCache)
                            .SelectMany(a =>
                            {
                                try
                                {
                                    return a.GetExportedTypes();
                                }
                                catch (TypeLoadException)
                                {
                                }
                                catch (ReflectionTypeLoadException)
                                {
                                }
                                catch (FileNotFoundException)
                                {
                                }
                                catch (FileLoadException)
                                {
                                }
                                return Enumerable.Empty<Type>();
                            })
                            .Where(t => !t.IsAbstract)
                            .Where(t => !t.IsInterface)
                            .Where(t => !t.IsGenericTypeDefinition);
        }
    }
}