﻿// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Reflection;

namespace Pocket;

/// <summary>
/// Provides support for registration of open generic types.
/// </summary>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
internal static class PocketContainerOpenGenericStrategy
{
    /// <summary>
    /// Registers an open generic type to another open generic type, allowing, for example, IService&amp;T&amp; to be registered to resolve to Service&amp;T&amp;.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="variantsOf">The open generic interface that callers will attempt to resolve, e.g. typeof(IService&amp;T&amp;).</param>
    /// <param name="to">The open generic type to resolve, e.g. typeof(Service&amp;T&amp;).</param>
    /// <param name="singletons">If true, each type will be lazily registered as a singleton.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException">
    /// Parameter 'variantsOf' is not an open generic type, e.g. typeof(IService&amp;T&amp;)
    /// or
    /// Parameter 'to' is not an open generic type, e.g. typeof(Service&amp;T&amp;)
    /// </exception>
    public static PocketContainer RegisterGeneric(this PocketContainer container, Type variantsOf, Type to, bool singletons = false)
    {
        if (!variantsOf.GetTypeInfo().IsGenericTypeDefinition)
        {
            throw new ArgumentException("Parameter 'variantsOf' is not an open generic type, e.g. typeof(IService<>)");
        }

        if (!to.GetTypeInfo().IsGenericTypeDefinition)
        {
            throw new ArgumentException("Parameter 'to' is not an open generic type, e.g. typeof(Service<>)");
        }

        return container.AddStrategy(t =>
        {
            if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == variantsOf)
            {
                var closedGenericType = to.MakeGenericType(t.GetTypeInfo().GenericTypeArguments);

                if (singletons)
                {
                    container.TryRegisterSingle(
                        t,
                        cc => cc.Resolve(closedGenericType)!);
                }

                return c => c.Resolve(closedGenericType);
            }
            return null;
        });
    }
}