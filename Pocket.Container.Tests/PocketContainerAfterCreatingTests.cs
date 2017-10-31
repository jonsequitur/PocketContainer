// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        public void AfterCreating_on_transient_registration_does_not_result_in_singleton_registration()
        {
            var container = new PocketContainer()
                .Register(c => new List<string>())
                .AfterCreating<List<string>>(list => list.Add("hi"));

            var resolved1 = container.Resolve<List<string>>();
            var resolved2 = container.Resolve<List<string>>();

            resolved2.Should().NotBeSameAs(resolved1);
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
        public void AfterCreating_can_be_called_before_Register()
        {
            // arrange
            var container = new PocketContainer()
                .AfterCreating<List<string>>(l =>
                {
                    l.Add("one");
                })
                .Register(c => new List<string> { "initial" });

            // act
            var list = container.Resolve<List<string>>();

            // assert
            list.Should()
                .BeEquivalentTo("initial", "one");
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
        public void AfterCreating_can_be_called_after_Register()
        {
            // arrange
            var container = new PocketContainer()
                .Register(c =>
                {
                    return new List<string> { "initial" };
                })
                .AfterCreating<List<string>>(l =>
                {
                    l.Add("one");
                });

            // act
            var list = container.Resolve<List<string>>();

            // assert
            list.Should()
                .BeEquivalentTo("initial", "one");
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

        [Fact]
        public void AfterCreating_can_be_called_before_and_after_Register()
        {
            var container = new PocketContainer()
                .AfterCreating<IList<string>>(c => c.Add("one"))
                .AfterCreating<IList<string>>(c => c.Add("two"))
                .Register<IList<string>>(c => new List<string> { "initial" })
                .AfterCreating<IList<string>>(c => c.Add("three"));

            container.Resolve<IList<string>>()
                     .Should()
                     .BeEquivalentTo("initial", "one", "two", "three");
        }

        [Fact]
        public void Register_can_be_called_multiple_times_after_AfterCreating()
        {
            var container = new PocketContainer()
                .AfterCreating<IList<string>>(c => c.Add("one"))
                .AfterCreating<IList<string>>(c => c.Add("two"))
                .Register<IList<string>>(c => new List<string> { "initial" })
                .Register<IList<string>>(c => new List<string> { "no, this initial" })
                .Register<IList<string>>(c => new List<string> { "ok, this initial" })
                .AfterCreating<IList<string>>(c => c.Add("three"));

            container.Resolve<IList<string>>()
                     .Should()
                     .BeEquivalentTo("ok, this initial", "one", "two", "three");
        }

        [Fact]
        public async Task AfterCreating_is_threadsafe()
        {
            var container = new PocketContainer()
                .Register<IList<string>>(c => new List<string>());

            var barrier = new Barrier(2);

            var task1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                container.AfterCreating<IList<string>>(list =>
                {
                    list.Add("one");
                });
            });

            var task2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                container.AfterCreating<IList<string>>(list =>
                {
                    list.Add("two");
                });
            });

            await Task.WhenAll(task1, task2);

            container.Resolve<IList<string>>()
                     .Should()
                     .Contain("one")
                     .And
                     .Contain("two");
        }
    }
}
