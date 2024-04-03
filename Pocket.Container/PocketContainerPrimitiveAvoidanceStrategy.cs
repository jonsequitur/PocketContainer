﻿// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pocket;

/// <summary>
/// Provides a strategy for choosing constructors that do not contain primitive types.
/// </summary>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
internal static class PocketContainerPrimitiveAvoidanceStrategy
{
    private static readonly MethodInfo pocketContainerResolveMethod = typeof (PocketContainer).GetMethod("Resolve", Type.EmptyTypes)!;

    private static readonly MethodInfo genericFuncFactoryMethod = typeof (PocketContainerPrimitiveAvoidanceStrategy)
        .GetMethod("UsingLongestConstructorHavingNoPrimitives", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly Type[] primitiveTypes = {
        typeof (string),
        typeof (DateTime),
        typeof (DateTimeOffset)
    };

    /// <summary>
    /// Configures a <see cref="PocketContainer" /> to prefer constructors that do not contain primitive types. 
    /// </summary>
    /// <remarks>Primitive types include any type for which <see cref="Type.IsPrimitive" /> is true, as well as <see cref="String" />, <see cref="DateTime" />, and <see cref="DateTimeOffset" />.</remarks>
    /// <param name="container">The same container instance.</param>
    public static PocketContainer AvoidConstructorsWithPrimitiveTypes(
        this PocketContainer container)
    {
        return container.AddStrategy(UseLongestConstructorHavingNoPrimitiveTypes);
    }

    private static Func<PocketContainer, object> UseLongestConstructorHavingNoPrimitiveTypes(
        Type forType)
    {
        var funcFactory = genericFuncFactoryMethod.MakeGenericMethod(forType);

        var func = funcFactory.Invoke(null, null);

        return (Func<PocketContainer, object>) func!;
    }

    // ReSharper disable once UnusedMember.Local
    private static Func<PocketContainer, T>? UsingLongestConstructorHavingNoPrimitives<T>()
    {
        var ctors = typeof (T).GetConstructors()
                              .OrderByDescending(c => c.GetParameters().Length)
                              .Where(c => !c.GetParameters().Any(p => p.ParameterType.IsPrimitive()))
                              .ToArray();

        if (!ctors.Any())
        {
            return null;
        }

        var longestCtorParamCount = ctors.Max(c => c.GetParameters().Length);

        var chosenCtor = ctors.Single(c => c.GetParameters().Length == longestCtorParamCount);

        var container = Expression.Parameter(typeof (PocketContainer), "container");

        var factoryExpr = Expression.Lambda<Func<PocketContainer, T>>(
            Expression.New(chosenCtor,
                           chosenCtor.GetParameters()
                                     .Select(p =>
                                                 Expression.Call(container,
                                                                 pocketContainerResolveMethod
                                                                     .MakeGenericMethod(p.ParameterType)))),
            container);

        return factoryExpr.Compile();
    }

    /// <summary>
    /// Determines whether the specified type is primitive.
    /// </summary>
    /// <remarks>This method defines primitive more broadly than the <see cref="Type.IsPrimitive" /> property. <see cref="String" />, for example, is considered primitive because as a type it carries no semantic information specific to a responsibility, so resolving it by convention will almost never be appropriate.</remarks>
    /// <param name="type">The type.</param>
    public static bool IsPrimitive(this Type type)
    {
        return type.GetTypeInfo().IsPrimitive ||
               primitiveTypes.Select(t => t.GetTypeInfo()).Contains(type.GetTypeInfo());
    }
}