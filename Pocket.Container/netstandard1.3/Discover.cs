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
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyModel;

namespace Pocket
{
    /// <summary>
    /// Performs runtime type discovery, useful for convention-based configuration tasks.
    /// </summary>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class Discover
    {
        public static IEnumerable<Type> DerivedFrom(
            this IEnumerable<Type> types,
            Type type) =>
            types.Where(t => type.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()));

        public static IEnumerable<Type> ConcreteTypes()
        {
            var types = Assemblies
                .Value
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
                .Where(t =>
                {
                    var typeInfo = t.GetTypeInfo();
                    return !typeInfo.IsAbstract &&
                           !typeInfo.IsInterface &&
                           !typeInfo.IsGenericTypeDefinition;
                });

            return types;
        }

        private static readonly Lazy<List<Assembly>> Assemblies = new Lazy<List<Assembly>>(() =>
        {
            var dependencyContext =
                //  DependencyContext.Load(typeof(Discover).GetTypeInfo().Assembly) ??
                DependencyContext.Default;

            var assemblyNames = dependencyContext
                .GetDefaultAssemblyNames();

            var assemblies = new List<Assembly>();
//            assemblies.Add(Assembly.GetEntryAssembly());
//            assemblies.Add(typeof(Discover).GetTypeInfo().Assembly);

            var defaultRuntimeLibraries = dependencyContext
                .RuntimeLibraries
                .Where(ReferencesCurrentAssembly);

            foreach (var library in assemblyNames)
            {
                try
                {
                    assemblies.Add(
                        Assembly.Load(
                            new AssemblyName(library.Name)));
                }
                catch (FileNotFoundException)
                {
                }
            }

            return assemblies;
        });

        private static readonly string currentAssemblyName = typeof(Discover).GetTypeInfo().Assembly.GetName().Name;

        private static bool ReferencesCurrentAssembly(
            RuntimeLibrary compilationLibrary)
        {
            if (compilationLibrary.Name.Equals(currentAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (compilationLibrary
                .Dependencies
                .Any(d => d.Name.StartsWith(currentAssemblyName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}

