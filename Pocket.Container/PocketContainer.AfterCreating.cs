// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Pocket
{
    internal partial class PocketContainer
    {
        public PocketContainer AfterCreating<T>(Func<T, T> transform)
        {
            TryRegisterSingle(c =>
                                  new AfterResolvePipeline<T>(this, ExistingResolver<T>()));

            var pipeline = Resolve<AfterResolvePipeline<T>>();

            pipeline.Transforms.Enqueue(transform);

            resolvers[typeof(T)] = c => pipeline.Resolve();

            if (pipeline.Transforms.Count == 1)
            {
                Registering += d => Reregister((dynamic) d);
            }

            return this;
        }

        private Func<PocketContainer, object> Reregister<T>(Func<PocketContainer, T> newResolverDelegate)
        {
            if (singletons.TryGetValue(typeof(AfterResolvePipeline<T>), out var pipelineObj))
            {
                var pipeline = (AfterResolvePipeline<T>) pipelineObj;

                if (newResolverDelegate is Func<PocketContainer, T> newResolver)
                {
                    var resolverType = newResolver.GetType().GenericTypeArguments[1];
                    var pipelineType = pipeline.GetType().GetGenericArguments()[0];
                    if (resolverType == pipelineType)
                    {
                        //                    Console.WriteLine($"RE-registering {transform} for {pipeline.ResolvesType}");

                        pipeline.InnerResolver = newResolver;
                    }
                    else
                    {
                        Console.WriteLine($"[HMMM] wanting to replace {resolverType} but pipeline is {pipelineType}");
                    }

                    return c => pipeline.Resolve();
                }
            }

            return null;
        }

        public PocketContainer AfterCreating<T>(Action<T> then) =>
            AfterCreating<T>(resolved =>
            {
                then(resolved);
                return resolved;
            });

        private class AfterResolvePipeline<T>
        {
            public readonly ConcurrentQueue<Func<T, T>> Transforms = new ConcurrentQueue<Func<T, T>>();

            private readonly PocketContainer container;

            private Func<PocketContainer, T> innerResolver;
            private bool transformsApplied;

            public AfterResolvePipeline(
                PocketContainer container,
                Func<PocketContainer, object> originalResolver)
            {
                this.container = container ?? throw new ArgumentNullException(nameof(container));

                if (originalResolver != null)
                {
                    InnerResolver = c => (T) originalResolver(c);
                }
            }

            public Type ResolvesType { get; } = typeof(T);

            public Func<PocketContainer, T> InnerResolver
            {
                private get
                {
                    if (innerResolver == null)
                    {
                        innerResolver = c =>
                        {
                            var implicitResolver = container.ImplicitResolver<T>();
                            return (T) implicitResolver(container);
                        };
                    }

                    return innerResolver;
                }
                set
                {
                    innerResolver = value;
                }
            }

            public T Resolve()
            {
                var instance = InnerResolver(container);

                var isSingleton =
                    container.singletons.TryGetValue(typeof(T), out var existing) &&
                    existing.Equals(instance);

                foreach (var transform in Transforms)
                {
                    if (isSingleton)
                    {
                        if (!transformsApplied)
                        {
                            var transformed = transform(instance);
                            if (transformed != null)
                            {
                                container.singletons.TryUpdate(typeof(T), transformed, instance);
                                instance = transformed;
                            }
                        }
                    }
                    else
                    {
                        var transformed = transform(instance);
                        if (transformed != null)
                        {
                            instance = transformed;
                        }
                    }
                }

                transformsApplied = true;

                return instance;
            }
        }

        private Func<PocketContainer, object> ExistingResolver<T>() =>
            this.Where(f => f.Key == typeof(T))
                .Select(pair => pair.Value)
                .SingleOrDefault();
    }
}
