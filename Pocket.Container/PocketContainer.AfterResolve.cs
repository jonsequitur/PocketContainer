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

namespace Pocket
{
    internal partial class PocketContainer
    {
        public PocketContainer AfterResolve<T>(Action<PocketContainer, T> then)
        {
            var applied = false;

            void Apply(Type type, object resolved)
            {
                if (type != typeof(T))
                {
                    return;
                }

                if (applied &&
                    singletons.TryGetValue(typeof(T), out var existing) &&
                    existing == resolved)
                {
                    return;
                }

                then(this, (T) resolved);

                applied = true;
            }

            OnResolved += Apply;

            return this;
        }
    }
}
