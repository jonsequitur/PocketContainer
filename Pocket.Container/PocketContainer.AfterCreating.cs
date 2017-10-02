// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;

namespace Pocket
{
    internal partial class PocketContainer
    {
        private Func<PocketContainer, object> Pipe<T>(
            Delegate resolve,
            Func<T, T> transform)
        {
            var applied = false;

            var f = (Func<PocketContainer, object>) resolve;

            return container =>
            {
                var resolved = f(container);

                if (singletons.TryGetValue(typeof(T), out var existing) &&
                    existing.Equals(resolved))
                {
                    if (!applied)
                    {
                        applied = true;
                        var transformed = transform((T) resolved);
                        singletons.TryUpdate(typeof(T), transformed, resolved);
                        resolved = transformed;
                    }
                }
                else
                {
                    var replaced = transform((T) resolved);

                    if (replaced != null)
                    {
                        resolved = replaced;
                    }
                }

                return resolved;
            };
        }

        private readonly ConcurrentDictionary<Type, Delegate> afterCreatingStash = new ConcurrentDictionary<Type, Delegate>();

        public PocketContainer AfterCreating<T>(Func<T, T> transform)
        {
            if (resolvers.TryGetValue(typeof(T), out var existing))
            {
                var f = Pipe(existing, transform);
                if (f != null)
                {
                    resolvers.TryUpdate(typeof(T), f, existing);
                }
            }
            else
            {
                afterCreatingStash.AddOrUpdate(
                    typeof(T),
                    t => transform,
                    (t, first) => new Func<T, T>(x =>
                    {
                        var transformed = ((Func<T, T>) first)(x);
                        return transform(transformed);
                    }));
            }

            Registering += f =>
            {
                if (afterCreatingStash.TryGetValue(typeof(T), out var stashed))
                {
                    return Pipe<T>(f, c => ((Func<T, T>) stashed).Invoke(c));
                }

                return null;
            };

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
