// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerAfterCreatingTests
    {
        [Fact]
        public void AfterCreating_can_be_used_to_get_an_existing_registration_and_modify_its_output_before_returning_it()
        {
            var container = new PocketContainer()
                .Register(c => new HashSet<string> { "initial" })
                .AfterCreating<HashSet<string>>(hashSet =>
                {
                    hashSet.Add("next");
                });

            var set = container.Resolve<HashSet<string>>();

            set.Should().Contain("initial");
            set.Should().Contain("next");
        }

        [Fact]
        public void AfterCreating_can_be_used_to_modify_the_output_of_a_default_registration()
        {
            var container = new PocketContainer()
                .AfterCreating<HasDefaultCtor<int>>(obj =>
                {
                    obj.Value++;
                });

            var resolved = container.Resolve<HasDefaultCtor<int>>();

            resolved.Value.Should().Be(1);
        }

        [Fact]
        public void AfterCreating_can_be_called_before_Register_but_still_applies()
        {
            var container = new PocketContainer()
                .AfterCreating<HashSet<string>>(hashSet =>
                {
                    hashSet.Add("next");
                })
                .Register(c => new HashSet<string> { "initial" });

            var set = container.Resolve<HashSet<string>>();

            set.Should().Contain("initial");
            set.Should().Contain("next");
        }

        [Fact]
        public void Multiple_AfterCreating_functions_can_be_applied()
        {
            var container = new PocketContainer()
                .Register(c => new HashSet<string> { "initial" })
                .AfterCreating<HashSet<string>>(hashSet =>
                {
                    hashSet.Add("one");
                })
                .AfterCreating<HashSet<string>>(hashSet =>
                {
                    hashSet.Add("two");
                });

            var set = container.Resolve<HashSet<string>>();

            set.Should().BeEquivalentTo(new object[] { "initial", "one", "two" });
        }

        [Fact]
        public void When_used_with_RegisterSingle_then_AfterCreating_is_only_called_once_per_instantiation()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasDefaultCtor<int>())
                .AfterCreating<HasDefaultCtor<int>>(obj =>
                {
                    obj.Value++;
                });

            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();

            var resolved = container.Resolve<HasDefaultCtor<int>>();

            resolved.Value.Should().Be(1);
        }

        [Fact]
        public void When_used_with_Register_then_AfterCreating_is_only_called_once_per_resolve()
        {
            var resolveCount = 0;

            var container = new PocketContainer()
                .Register(c => new HasDefaultCtor<int>())
                .AfterCreating<HasDefaultCtor<int>>(obj =>
                {
                    resolveCount++;
                    obj.Value++;
                });

            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();
            container.Resolve<HasDefaultCtor<int>>();

            resolveCount.Should().Be(3);
        }

        [Fact]
        public void AfterCreating_type_matching_is_precise()
        {
            var container = new PocketContainer()
                .Register<IList<string>>(c => new List<string>())
                .Register<List<string>>(c => new List<string>())
                .AfterCreating<List<string>>(l =>
                {
                    l.Add("List");
                })
                .AfterCreating<IList<string>>(l =>
                {
                    l.Add("IList");
                });

            var ilist = container.Resolve<IList<string>>();
            var list = container.Resolve<List<string>>();

            ilist.Should()
                 .HaveCount(1)
                 .And
                 .OnlyContain(item => item == "IList");

            list.Should()
                .HaveCount(1)
                .And
                .OnlyContain(item => item == "List");
        }
    }
}
