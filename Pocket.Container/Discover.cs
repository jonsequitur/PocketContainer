// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Pocket;

/// <summary>
/// Performs runtime type discovery, useful for convention-based configuration tasks.
/// </summary>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
internal static class Discover
{
    /// <summary>
    ///  Filters to types that can be instantiated.
    /// </summary>
    public static IEnumerable<Type> Concrete(this IEnumerable<Type> types) =>
        types.Where(t => !t.IsAbstract &&
                         !t.IsEnum &&
                         !typeof(Delegate).IsAssignableFrom(t) &&
                         !t.IsInterface &&
                         !t.IsGenericTypeDefinition);

    /// <summary>
    /// Discovers concrete types within the current AppDomain.
    /// </summary>
    public static IEnumerable<Type> ConcreteTypes() =>
        Types().Concrete();

    /// <summary>
    /// Filters to types that are derived from the specified type.
    /// </summary>
    public static IEnumerable<Type> DerivedFrom(this IEnumerable<Type> types, Type type) =>
        types.Where(type.IsAssignableFrom);

    /// <summary>
    /// Filters to types that implement a generic variant of one of the specified open generic interfaces.
    /// </summary>
    public static IEnumerable<Type> ImplementingOpenGenericInterfaces(
        this IEnumerable<Type> source,
        params Type[] interfaces) =>
        source.Where(t => t.GetInterfaces()
                           .Any(i => i.IsConstructedGenericType &&
                                     interfaces.Contains(i.GetGenericTypeDefinition())));

    /// <summary>
    /// Gets types within the current AppDomain.
    /// </summary>
    public static IEnumerable<Type> Types() =>
        AppDomain.CurrentDomain
                 .GetAssemblies()
                 .Where(a => !a.IsDynamic)
#if NETFRAMEWORK
                     .Where(a => !a.GlobalAssemblyCache)
#endif
                 .Types();

    /// <summary>
    /// Gets types within the specified assemblies.
    /// </summary>
    public static IEnumerable<Type> Types(this IEnumerable<Assembly> assemblies) =>
        assemblies.SelectMany(a =>
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