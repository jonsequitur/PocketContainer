// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;

namespace Pocket
{
    internal partial class PocketContainer
    {
        /// <summary>
        /// Clones the container, allowing for selectively overriding registrations without affecting the state of the original container.
        /// </summary>
        public PocketContainer Clone() =>
            new PocketContainer
            {
                resolvers = new ConcurrentDictionary<Type, Func<PocketContainer, object>>(resolvers),
                strategyChain = strategyChain,
                singletons = new ConcurrentDictionary<Type, object>(singletons)
            };
    }
}
