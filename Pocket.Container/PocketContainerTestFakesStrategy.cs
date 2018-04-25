// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Pocket;

namespace Pocket
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class PocketContainerTestFakesStrategy
    {
        public static Pocket.PocketContainer UseFakeTypesFromAssembly(this PocketContainer container, Assembly assembly)
            => container.UseTypesFromAssembly(assembly, t => Regex.IsMatch(t.FullName, @".+Fake$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        public static Pocket.PocketContainer UseTypesFromAssembly(this PocketContainer container, Assembly assembly, Func<Type, bool> convention)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (convention == null) throw new ArgumentNullException(nameof(convention));

            return container.AddStrategy(type =>
            {
                var fakes = Discover
                    .ConcreteTypes()
                    .DerivedFrom(type)
                    .Where(t => t.Assembly == assembly && convention(t))
                    .CloseTo(type)
                    .ToArray();

                var fake = fakes.FirstOrDefault();


                if (fake != null)
                {
                    return c => c.Resolve(fake);
                }
                return null;
            });
        }


        public static Pocket.PocketContainer UseFakeTypesCloseTo<T>(this PocketContainer container, T _)
            => container.UseFakeTypesCloseTo<T>();

        public static Pocket.PocketContainer UseTypesCloseTo<T>(this PocketContainer container, T _, Func<Type, bool> convention)
            => container.UseTypesCloseTo<T>(convention);

        public static Pocket.PocketContainer UseFakeTypesCloseTo<T>(this PocketContainer container)
            => container.UseFakeTypesCloseTo(typeof(T));

        public static Pocket.PocketContainer UseTypesCloseTo<T>(this PocketContainer container, Func<Type, bool> convention)
            => container.UseTypesCloseTo(typeof(T), convention);

        public static Pocket.PocketContainer UseFakeTypesCloseTo(this PocketContainer container, Type searchPoint)
            => container.UseTypesCloseTo(searchPoint, t => Regex.IsMatch(t.FullName, @".+Fake$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        public static Pocket.PocketContainer UseTypesCloseTo(this PocketContainer container, Type searchPoint, Func<Type, bool> convention)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (searchPoint == null)
            {
                throw new ArgumentNullException(nameof(searchPoint));
            }

            if (convention == null) throw new ArgumentNullException(nameof(convention));


            return container.AddStrategy(type =>
            {
                var fakes = Discover
                        .ConcreteTypes()
                        .DerivedFrom(type)
                        .Where(convention)
                        .CloseTo(searchPoint).ToArray();

                    var fake = fakes.FirstOrDefault();


                
                if (fake != null)
                {
                    return c => c.Resolve(fake);
                }
                return null;
            });
        }
    }
}
