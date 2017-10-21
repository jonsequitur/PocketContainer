using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pocket
{
    internal static class PocketContainerCumulativeRegistration
    {
        private static readonly AsyncLocal<int> recursionCounter = new AsyncLocal<int>();

        public static PocketContainer AccumulateRegistrations(
            this PocketContainer container)
        {
            container.Registering += (type, resolver) =>
            {
                AddFactoryToList(container, (dynamic) resolver);
                return resolver;
            };

            return container;
        }

        private static void AddFactoryToList<T>(
            PocketContainer container,
            Func<PocketContainer, T> factory)
        {
            // avoid re-entrancy which would result in a stack overflow
            if (recursionCounter.Value != 0)
            {
                return;
            }

            try
            {
                recursionCounter.Value++;

                // register IEnumerable<Func<PocketContainer, T>>
                container.TryRegister(c => c.Resolve<List<Func<PocketContainer, T>>>()
                                            .Select(f => f(c)));

                // register the registration list as a singleton
                container.TryRegisterSingle(c => new List<Func<PocketContainer, T>>());

                // resolve it and add the factory
                var registrations = container.Resolve<List<Func<PocketContainer, T>>>();

                registrations.Add(factory);
            }
            finally
            {
                recursionCounter.Value--;
            }
        }
    }
}
