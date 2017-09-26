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
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Pocket
{
    internal partial class PocketContainer :
        IServiceProvider,
        ISupportRequiredService
    {
        partial void AfterConstructor() =>
            OnFailedResolve = (type, exception) => null;

        /// <summary>Gets the service object of the specified type.</summary>
        /// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param>
        /// <filterpriority>2</filterpriority>
        public object GetService(Type serviceType) =>
            Resolve(serviceType);

        /// <summary>
        /// Gets service of type <paramref name="serviceType" /> from the <see cref="T:System.IServiceProvider" /> implementing
        /// this interface.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.
        /// Throws an exception if the <see cref="T:System.IServiceProvider" /> cannot create the object.</returns>
        public object GetRequiredService(Type serviceType) =>
            Resolve(serviceType) ??
            throw new ArgumentNullException($"Service of type {serviceType} is not registered.");

        public event Action<(Type serviceType, object resolved)> OnResolved;

        partial void AfterResolve(Type type, object resolved) =>
            OnResolved?.Invoke((type, resolved));

        public bool HasSingletonOfType(Type type) => singletons.ContainsKey(type);
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
    }

    internal class ServiceScopeFactory : IServiceScopeFactory
    {
        private readonly PocketContainer container;

        public ServiceScopeFactory(PocketContainer container)
        {
            this.container = container ??
                             throw new ArgumentNullException(nameof(container));
        }

        public IServiceScope CreateScope() => new ServiceScope(container);
    }

    internal class ServiceScope : IServiceScope
    {
        private readonly PocketContainer container;

        private readonly CompositeDisposable disposables;

        private bool isDisposed;

        public ServiceScope(PocketContainer container)
        {
            this.container = container?.Clone() ??
                             throw new ArgumentNullException(nameof(container));

            this.container.OnResolved += OnResolved;

            disposables = new CompositeDisposable
            {
                () => this.container.OnResolved -= OnResolved,
                () => isDisposed = true
            };
        }

        private void OnResolved(
            Type serviceType,
            object service)
        {
            if (service is IDisposable disposable)
            {
                disposables.Add(() =>
                {
                    if (!container.HasSingletonOfType(serviceType))
                    {
                        disposable.Dispose();
                    }
                });
            }
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
                return container;
            }
        }
    }
}
