// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// THIS FILE IS NOT INTENDED TO BE EDITED. 
// 
// It has been imported using NuGet from the PocketContainer project (https://github.com/jonsequitur/PocketContainer). 
// 
// This file can be updated in-place using the Package Manager Console. To check for updates, run the following command:
// 
// PM> Get-Package -Updates

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pocket
{
#if (NET451 || NET452 || NET461 || NET462)

    /// <summary>
    /// Performs runtime type discovery, useful for convention-based configuration tasks.
    /// </summary>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
    [ExcludeFromCodeCoverage]
#endif
    internal static class Discover
    {
        public static IEnumerable<Type> DerivedFrom(this IEnumerable<Type> types, Type type) =>
            types.Where(type.IsAssignableFrom);

        public static IEnumerable<Type> ConcreteTypes(bool includeGlobalAssemblyCache = false) =>
            Types(includeGlobalAssemblyCache)
                .Where(t => !t.IsAbstract &&
                            !t.IsInterface &&
                            !t.IsGenericTypeDefinition);

        private static IEnumerable<Type> Types(bool includeGlobalAssemblyCache = false) =>
            AppDomain.CurrentDomain
                     .GetAssemblies()
                     .Where(a => !a.IsDynamic)
                     .Where(a => includeGlobalAssemblyCache || !a.GlobalAssemblyCache)
                     .SelectMany(a =>
                     {
                         try
                         {
                             return a.DefinedTypes;
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
                     });
    }
#endif
}
