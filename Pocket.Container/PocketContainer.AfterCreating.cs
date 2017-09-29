// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Pocket
{
    internal partial class PocketContainer
    {
        public PocketContainer AfterCreating<T>(Func<T, T> then)
        {
            var applied = false;

            object Apply(Type type, object resolved)
            {
                if (type != typeof(T))
                {
                    return resolved;
                }

                if (applied &&
                    singletons.TryGetValue(typeof(T), out var existing) &&
                    existing.Equals(resolved))
                {
                    return resolved;
                }

                applied = true;

                return then((T) resolved);
            }

            AfterResolve += Apply;

            return this;
        }

        public PocketContainer AfterCreating<T>(Action<T> then)
        {
            return AfterCreating<T>(resolved =>
            {
                then(resolved);
                return resolved;
            });
        }
    }
}
