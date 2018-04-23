// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Pocket;

namespace PocketContainer
{
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal static class PocketContainerTestFakesStrategy
    {
        public static Pocket.PocketContainer ResolveWithFakeTypesFromAssembly(
            this Pocket.PocketContainer container, Assembly assembly)
        {
            return container.ResolveWithFakeTypesFromAssembly(assembly, "Fake");
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesFromAssembly(
            this Pocket.PocketContainer container, Assembly assembly, string convention)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(convention))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(convention));

            var conventionFilter = convention.ToLower(CultureInfo.InvariantCulture);

            return container.AddStrategy(type =>
            {
                Type fake = null;
                if (!type.FullName.EndsWith(conventionFilter, StringComparison.InvariantCultureIgnoreCase))
                {
                    var fakes = Discover
                        .ConcreteTypes()
                        .DerivedFrom(type)
                        .Where(t => t.Assembly == assembly && t.FullName.EndsWith(conventionFilter,
                                        StringComparison.InvariantCultureIgnoreCase))
                        .OrderBy(t => TypeDistanceFrom(type, t))
                        .ToArray();

                    fake = fakes.FirstOrDefault();
                }

                if (fake != null)
                {
                    return c => c.Resolve(fake);
                }
                return null;
            });
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo(
            this Pocket.PocketContainer container, Type searchPoint)
        {
            return container.ResolveWithFakeTypesCloseTo(searchPoint, "Fake");
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo<T>(
            this Pocket.PocketContainer container, T _)
        {
            return container.ResolveWithFakeTypesCloseTo<T>("Fake");
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo<T>(
            this Pocket.PocketContainer container, T _, string convention)
        {
            return container.ResolveWithFakeTypesCloseTo<T>(convention);
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo<T>(
            this Pocket.PocketContainer container)
        {
            return container.ResolveWithFakeTypesCloseTo<T>("Fake");
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo<T>(
            this Pocket.PocketContainer container, string convention)
        {
            return container.ResolveWithFakeTypesCloseTo(typeof(T), convention);
        }

        public static Pocket.PocketContainer ResolveWithFakeTypesCloseTo(
            this Pocket.PocketContainer container, Type searchPoint, string convention)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (searchPoint == null) throw new ArgumentNullException(nameof(searchPoint));
            if (string.IsNullOrWhiteSpace(convention))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(convention));

            var conventionFilter = convention.ToLower(CultureInfo.InvariantCulture);
            return container.AddStrategy(type =>
            {
                Type fake = null;
                if (!type.FullName.EndsWith(conventionFilter, StringComparison.InvariantCultureIgnoreCase))
                {
                    var fakes = Discover
                        .ConcreteTypes()
                        .DerivedFrom(type)
                        .Where(t => t.FullName.EndsWith(conventionFilter, StringComparison.InvariantCultureIgnoreCase))
                        .OrderBy(t => TypeDistanceFrom(searchPoint, t)).ToArray();
                        
                        fake = fakes.FirstOrDefault();

                    
                }
                if (fake != null)
                {
                    return c => c.Resolve(fake);
                }
                return null;
            });
        }

        private static int TypeDistanceFrom(Type searchPoint, Type element)
        {
            
            var distance = 0;
            if (element.IsNested && element.DeclaringType == searchPoint)
            {
                return distance;
            }
            var sameAssmeby = searchPoint.Assembly == element.Assembly;
            var searchPointPath = searchPoint.FullName.Split(new[] {'.', '+'}, StringSplitOptions.RemoveEmptyEntries);
            var elementPath = element.FullName.Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries);
            var scanLimit = Math.Min(elementPath.Length - 1, searchPointPath.Length);
            var maxDistance = Math.Max(elementPath.Length - 1, searchPointPath.Length);
            for (var i = 0; i < scanLimit; i++)
            {
                if (elementPath[i] != searchPointPath[i])
                {
                    distance = maxDistance - i;
                    break;
                }
            }

            return distance = sameAssmeby ? distance : distance << 4;
        }
    }
}
