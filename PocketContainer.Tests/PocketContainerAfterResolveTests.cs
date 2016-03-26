// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerAfterResolveTests
    {
        [Test]
        public void AfterResolve_can_be_used_to_get_an_existing_registration_and_modify_its_output_before_returning_it()
        {
            var container = new PocketContainer()
                .Register(c =>
                {
                    var hashSet = new HashSet<string> { "initial" };
                    return hashSet;
                })
                .AfterResolve<HashSet<string>>((c, hashSet) =>
                {
                    hashSet.Add("next");
                    return hashSet;
                });

            var set = container.Resolve<HashSet<string>>();

            set.Should().Contain("initial");
            set.Should().Contain("next");
        }

        [Test]
        public void AfterResolve_can_be_used_to_modify_the_output_of_a_default_registration()
        {
            var container = new PocketContainer()
                .AfterResolve<HasDefaultCtor<int>>((c, obj) =>
                {
                    obj.Value ++;
                    return obj;
                });

            var resolved = container.Resolve<HasDefaultCtor<int>>();

            resolved.Value.Should().Be(1);
        }

        [Ignore("Not implemented")]
        [Test]
        public void AfterResolve_can_be_called_before_Register_but_still_applies()
        {
            var container = new PocketContainer()
                .AfterResolve<HashSet<string>>((c, hashSet) =>
                {
                    hashSet.Add("next");
                    return hashSet;
                })
                .Register(c =>
                {
                    var hashSet = new HashSet<string> { "initial" };
                    return hashSet;
                });

            var set = container.Resolve<HashSet<string>>();

            set.Should().Contain("initial");
            set.Should().Contain("next");
        }

        [Test]
        public void Multiple_AfterResolve_functions_can_be_applied()
        {
            var container = new PocketContainer()
                .Register(c =>
                {
                    var hashSet = new HashSet<string> { "initial" };
                    return hashSet;
                })
                .AfterResolve<HashSet<string>>((c, hashSet) =>
                {
                    hashSet.Add("one");
                    return hashSet;
                })
                .AfterResolve<HashSet<string>>((c, hashSet) =>
                {
                    hashSet.Add("two");
                    return hashSet;
                });

            var set = container.Resolve<HashSet<string>>();

            set.Should().BeEquivalentTo(new object[] { "initial", "one", "two" });
        }

        [Test]
        public void When_used_with_RegisterSingle_then_AfterResolve_is_only_called_once_per_instantiation()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasDefaultCtor<int>())
                .AfterResolve<HasDefaultCtor<int>>((c, obj) =>
                {
                    obj.Value ++;
                    return obj;
                });

            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();

            var resolved = container.Resolve<HasDefaultCtor<int>>();

            resolved.Value.Should().Be(1);
        }
        
        [Test]
        public void When_used_with_Register_then_AfterResolve_is_only_called_once_per_resolve()
        {
            var container = new PocketContainer()
                .Register(c => new HasDefaultCtor<int>())
                .AfterResolve<HasDefaultCtor<int>>((c, obj) =>
                {
                    obj.Value ++;
                    return obj;
                });

            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();

            var resolved = container.Resolve<HasDefaultCtor<int>>();

            resolved.Value.Should().Be(3);
        }
    }
}