// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Linq;

namespace Pocket
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class PocketContainerSingleImplementationStrategy
    {
        /// <summary>
        /// Adds a strategy to PocketContainer whereby interfaces having only a single implementation do not need explicit registration in order to be resolvable.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The same container instance.</returns>
        public static PocketContainer IfOnlyOneImplementationUseIt(
            this PocketContainer container)
        {
            return container.AddStrategy(type =>
            {
                if (type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsAbstract)
                {
                    var implementations = Discover.ConcreteTypes()
                                                  .DerivedFrom(type)
                                                  .ToArray();

                    if (implementations.Length == 1)
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
