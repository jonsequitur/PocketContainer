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
            throw new ArgumentNullException($"Service of type {serviceType} is not registerd.");
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

            container.OnFailedResolve = (type, exception) => null;

            return container;
        }

        private static void Register(
            this PocketContainer container,
            ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                container.Register(
                    descriptor.ServiceType,
                    c => descriptor.ImplementationInstance);
            }

            if (descriptor.ImplementationType != null)
            {
                container.Register(
                    descriptor.ServiceType,
                    c => c.Resolve(descriptor.ImplementationType));
            }

            if (descriptor.ImplementationFactory != null)
            {
                container.Register(
                    descriptor.ServiceType,
                    c => descriptor.ImplementationFactory(c));
            }
        }
    }
}
