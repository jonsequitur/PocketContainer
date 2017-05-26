using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pocket
{
    internal partial class PocketContainer
    {
        private static readonly ThreadLocal<int> recursionCounter = new ThreadLocal<int>(() => 0);

        partial void BeforeRegister<T>(Func<PocketContainer, T> factory)
        {
            // avoid re-entrancy which would result in a stack overflow
            if (recursionCounter.Value == 0)
            {
                try
                {
                    recursionCounter.Value++;

                    TryRegister(c => c.Resolve<List<Func<PocketContainer, T>>>()
                                               .Select(f => f(c)));

                    TryRegisterSingle(c => new List<Func<PocketContainer, T>>());

                    var registrations = Resolve<List<Func<PocketContainer, T>>>();

                    registrations.Add(factory);
                }
                finally
                {
                    recursionCounter.Value--;
                }
            }
        }
    }
}
