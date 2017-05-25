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
using System.Collections.Concurrent;
using System.Linq;

namespace Pocket
{
    internal partial class PocketContainer
    {
        public PocketContainer AfterResolve<T>(Func<PocketContainer, T, T> then)
        {
            var pipeline = new AfterResolvePipeline<T>(this, GetResolver<T>());
            pipeline.Transforms.Enqueue(then);
            Register(typeof (T), c => pipeline.Resolve());
            return this;
        }

        private class AfterResolvePipeline<T>
        {
            public readonly ConcurrentQueue<Func<PocketContainer, T, T>> Transforms = new ConcurrentQueue<Func<PocketContainer, T, T>>();
            private readonly PocketContainer container;
            private readonly Func<PocketContainer, object> originalRegistration;

            public AfterResolvePipeline(
                PocketContainer container, 
                Func<PocketContainer, object> originalResolver)
            {
                if (container == null)
                {
                    throw new ArgumentNullException("container");
                }
                this.container = container;
                originalRegistration = originalResolver;
            }

            public T Resolve()
            {
                var instance = originalRegistration(container);

                var transformed = (T) Transforms.Aggregate(instance,
                                                           (current, transform) => transform(container, (T) current));

                return transformed;
            }
        }

        private Func<PocketContainer, object> GetResolver<T>()
        {
            var existing = this.Where(f => f.Key == typeof (T))
                               .Select(pair => pair.Value)
                               .SingleOrDefault();

            if (existing != null)
            {
                return existing;
            }

            // trigger the creation of one:
            var instance = Resolve<T>();

            return GetResolver<T>();
        }
    }
}