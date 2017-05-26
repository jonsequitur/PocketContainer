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
using System.Linq;
using System.Reflection;

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
                        return c => c.Resolve(implementations.Single());
                    }
                }
                return null;
            });
        }
    }
}