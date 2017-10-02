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
        public void AfterCreating_can_be_used_to_get_an_existing_registration_and_modify_its_output_before_returning_it_when_using_Resolve_T()
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
        public void AfterCreating_can_be_used_to_get_an_existing_registration_and_modify_its_output_before_returning_it_when_using_Resolve()
        {
            var container = new PocketContainer()
                .Register(c => new HashSet<string> { "initial" })
                .AfterCreating<HashSet<string>>(hashSet =>
                {
                    hashSet.Add("next");
                });

            var set = (HashSet<string>) container.Resolve(typeof(HashSet<string>));

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
        public void When_used_after_RegisterSingle_then_transform_is_only_called_once_per_instantiation()
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
        public void When_used_before_RegisterSingle_then_transform_is_only_called_once_per_instantiation()
        {
            var container = new PocketContainer()
                .AfterCreating<HasDefaultCtor<int>>(obj =>
                {
                    obj.Value++;
                })
                .RegisterSingle(c => new HasDefaultCtor<int>());

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

        [Fact]
        public void AfterCreating_can_be_used_to_replace_the_returned_instance_when_using_Resolve_T()
        {
            var container = new PocketContainer()
                .Register(c => "before")
                .AfterCreating<string>(instance => "after");

            var resolved = container.Resolve<string>();

            resolved.Should().Be("after");
        }

        [Fact]
        public void AfterCreating_can_be_used_to_replace_the_returned_instance_when_using_Resolve()
        {
            var container = new PocketContainer()
                .Register(c => "before")
                .AfterCreating<string>(instance => "after");

            var resolved = (string) container.Resolve(typeof(string));

            resolved.Should().Be("after");
        }

        [Fact]
        public void When_used_with_RegisterSingleton_then_AfterCreating_can_be_used_to_replace_the_returned_instance_when_resolved_using_generic_Resolve()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasOneParamCtor<string>("before"))
                .AfterCreating<HasOneParamCtor<string>>(instance => new HasOneParamCtor<string>("after"));

            var resolved1 = container.Resolve<HasOneParamCtor<string>>();
            var resolved2 = container.Resolve<HasOneParamCtor<string>>();

            resolved1.Value1.Should().Be("after");
            resolved2.Should().BeSameAs(resolved1);
        }

        [Fact]
        public void When_used_with_RegisterSingleton_then_AfterCreating_can_be_used_to_replace_the_returned_instance_when_resolved_using_non_generic_Resolve()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasOneParamCtor<string>("before"))
                .AfterCreating<HasOneParamCtor<string>>(instance => new HasOneParamCtor<string>("after"));

            var resolved1 = container.Resolve(typeof(HasOneParamCtor<string>)) as HasOneParamCtor<string>;
            var resolved2 = container.Resolve(typeof(HasOneParamCtor<string>)) as HasOneParamCtor<string>;

            resolved1.Value1.Should().Be("after");
            resolved2.Should().BeSameAs(resolved1);
        }

        [Fact]
        public void AfterCreating_can_be_called_multiple_times_before_Register()
        {
            var container = new PocketContainer()
                .AfterCreating<IList<string>>(c => c.Add("one"))
                .AfterCreating<IList<string>>(c => c.Add("two"))
                .AfterCreating<IList<string>>(c => c.Add("three"))
                .Register<IList<string>>(c => new List<string> { "initial" });

            container.Resolve<IList<string>>()
                     .Should()
                     .BeEquivalentTo("initial", "one", "two", "three");
        }

        [Fact]
        public void AfterCreating_can_be_called_multiple_times_after_Register()
        {
            var container = new PocketContainer()
                .Register<IList<string>>(c => new List<string> { "initial" })
                .AfterCreating<IList<string>>(c => c.Add("one"))
                .AfterCreating<IList<string>>(c => c.Add("two"))
                .AfterCreating<IList<string>>(c => c.Add("three"));

            container.Resolve<IList<string>>()
                     .Should()
                     .BeEquivalentTo("initial", "one", "two", "three");
        }
    }
}
