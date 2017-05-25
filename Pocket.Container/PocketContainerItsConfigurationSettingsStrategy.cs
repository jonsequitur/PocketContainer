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
using Its.Configuration;
using Pocket;

namespace Microsoft.Its.Configuration
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
    internal static class PocketContainerItsConfigurationSettingsStrategy
    {
        /// <summary>
        /// Uses Its.Configuration to resolve unregistered types whose name ends with "Settings".
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The same container instance.</returns>
        public static PocketContainer UseItsConfigurationForSettings(
            this PocketContainer container)
        {
            return container.AddStrategy(type =>
            {
                if (!type.IsInterface &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition &&
                    type.Name.EndsWith("Settings"))
                {
                    return c => Settings.Get(type);
                }
                return null;
            });
        }
    }
}