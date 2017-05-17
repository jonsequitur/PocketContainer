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

namespace Pocket
{
    internal partial class PocketContainer
    {
        public PocketContainer TryRegister(
            Type type, 
            Func<PocketContainer, object> factory)
        {
            if (!resolvers.ContainsKey(type))
            {
                Register(type, factory);
            }

            return this;
        }

        public PocketContainer TryRegister<T>(Func<PocketContainer, T> factory)
        {
            if (!resolvers.ContainsKey(typeof(T)))
            {
                Register(factory);
            }

            return this;
        }

        public PocketContainer TryRegisterSingle(
            Type type, 
            Func<PocketContainer, object> factory)
        {
            if (!resolvers.ContainsKey(type))
            {
                RegisterSingle(type, factory);
            }

            return this;
        }

        public PocketContainer TryRegisterSingle<T>(Func<PocketContainer, T> factory)
        {
            if (!resolvers.ContainsKey(typeof(T)))
            {
                RegisterSingle(factory);
            }

            return this;
        }
    }
}
