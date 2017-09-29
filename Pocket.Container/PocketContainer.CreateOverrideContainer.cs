// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Pocket
{
    internal partial class PocketContainer
    {
        /// <summary>
        /// Clones the container, allowing for selectively overriding registrations.
        /// </summary>
        public PocketContainer CreateOverrideContainer()
        {
            var fallback = this;

            var child = Clone();

            return child.AddStrategy(t =>
            {
                // if the parent already has a registation, use it
                if (fallback.resolvers.TryGetValue(t, out var resolver))
                {
                    return resolver;
                }

                return null;
            });
        }
    }
}