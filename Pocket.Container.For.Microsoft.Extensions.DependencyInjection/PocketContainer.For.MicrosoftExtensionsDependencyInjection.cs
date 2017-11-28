// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Pocket
{
    internal partial class PocketContainer :
        IServiceProvider,
        ISupportRequiredService,
        IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        partial void AfterConstructor() =>
            OnFailedResolve = (type, exception) => null;

        /// <inheritdoc />
        public object GetService(Type serviceType) =>
            Resolve(serviceType);

        /// <inheritdoc />
        public object GetRequiredService(Type serviceType) =>
            Resolve(serviceType) ??
            throw new ArgumentNullException($"Service of type {serviceType} is not registered.");

        public bool HasSingletonOfType(Type type) => singletons.ContainsKey(type);

        internal object RegisterSingletonForDisposal(Type serviceType, object service)
        {
            if (service is IDisposable disposable)
            {
                disposables.Add(() =>
                {
                    if (HasSingletonOfType(serviceType))
                    {
                        disposable.Dispose();
                    }
                });
            }

            return service;
        }

        public void Dispose() => disposables.Dispose();
    }

    internal static class MicrosoftDependencyInjectionExtensions
    {
        public static IServiceProvider AsServiceProvider(
            this PocketContainer container,
            IServiceCollection services)
        {
            foreach (var service in services)
            {
                Register(container, service);
            }

            container.RegisterSingle<IServiceProvider>(c => container)
                     .RegisterSingle<IServiceScopeFactory>(c => new ServiceScopeFactory(c));

            container.AfterResolve += container.RegisterSingletonForDisposal;

            container.OnFailedResolve = (type, exception) => null;

            return container;
        }

        private static void Register(
            this PocketContainer container,
            ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                container.RegisterSingle(
                    descriptor.ServiceType,
                    c => descriptor.ImplementationInstance);
                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                if (descriptor.Lifetime != ServiceLifetime.Singleton)
                {
                    container.Register(
                        descriptor.ServiceType,
                        c => descriptor.ImplementationFactory(c));
                }
                else
                {
                    container.RegisterSingle(
                        descriptor.ServiceType,
                        c => descriptor.ImplementationFactory(c));
                }
                return;
            }

            if (descriptor.ImplementationType != null)
            {
                if (descriptor.IsOpenGeneric())
                {
                    container.RegisterGeneric(
                        variantsOf: descriptor.ServiceType,
                        to: descriptor.ImplementationType);
                }
                else if (descriptor.ServiceType == descriptor.ImplementationType)
                {
                    // no need to register it
                }
                else
                {
                    if (descriptor.Lifetime != ServiceLifetime.Singleton)
                    {
                        container.Register(
                            descriptor.ServiceType,
                            c => c.Resolve(descriptor.ImplementationType));
                    }
                    else
                    {
                        container.RegisterSingle(
                            descriptor.ServiceType,
                            c => c.Resolve(descriptor.ImplementationType));
                    }
                }
            }
        }

        public static bool IsOpenGeneric(this ServiceDescriptor descriptor) =>
            descriptor.ServiceType.GetTypeInfo().IsGenericTypeDefinition;

        private class ServiceScopeFactory : IServiceScopeFactory
        {
            private readonly PocketContainer container;

            public ServiceScopeFactory(PocketContainer container)
            {
                this.container = container ??
                                 throw new ArgumentNullException(nameof(container));
            }

            public IServiceScope CreateScope() => new ServiceScope(container);
        }

        private class ServiceScope : IServiceScope
        {
            private readonly PocketContainer clonedContainer;

            private readonly PocketContainer originalContainer;

            private readonly CompositeDisposable disposables;

            private bool isDisposed;

            public ServiceScope(PocketContainer container)
            {
                originalContainer = container ??
                                    throw new ArgumentNullException(nameof(container));
                clonedContainer = container.Clone();

                clonedContainer.AfterResolve += RegisterTransientsForDisposal;

                disposables = new CompositeDisposable
                {
                    () => clonedContainer.AfterResolve -= RegisterTransientsForDisposal,
                    () => isDisposed = true
                };
            }

            private object RegisterTransientsForDisposal(
                Type serviceType,
                object service)
            {
                if (service is IDisposable disposable)
                {
                    disposables.Add(() =>
                    {
                        if (!clonedContainer.HasSingletonOfType(serviceType) &&
                            !originalContainer.HasSingletonOfType(serviceType))
                        {
                            disposable.Dispose();
                        }
                    });
                }

                return service;
            }

            public void Dispose() => disposables.Dispose();

            public IServiceProvider ServiceProvider
            {
                get
                {
                    if (isDisposed)
                    {
                        throw new ObjectDisposedException("The ServiceScope has been disposed.");
                    }
                    return clonedContainer;
                }
            }
        }
    }
}
