// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerCloneTests
    {
        [Fact]
        public void Top_level_instances_are_resolved_from_the_original_container_when_the_clone_has_no_registration()
        {
            var original = new PocketContainer()
                .Register(c => "from original");

            var clone = original.Clone();

            clone.Resolve<string>()
                 .Should()
                 .Be("from original");
        }

        [Fact]
        public void Dependencies_are_resolved_from_the_original_container_when_the_clone_has_no_registration()
        {
            var original = new PocketContainer()
                .Register(c => "from original");

            var clone = original.Clone();

            clone.Resolve<HasOneParamCtor<string>>()
                 .Value1
                 .Should()
                 .Be("from original");
        }

        [Fact]
        public void Top_level_instances_are_resolved_from_the_clone_when_it_has_a_registration()
        {
            var original = new PocketContainer();

            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<string>().Should().Be("from clone");
        }

        [Fact]
        public void Dependencies_are_resolved_from_the_clone_when_it_has_a_registration()
        {
            var original = new PocketContainer();

            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<HasOneParamCtor<string>>()
                 .Value1
                 .Should()
                 .Be("from clone");
        }

        [Fact]
        public void Dependencies_of_a_registration_from_the_original_can_be_resolved_from_the_clone()
        {
            var original = new PocketContainer()
                .Register<IAmAnInterface>(c => c.Resolve<HasOneParamCtor<string>>());

            var clone = original.Clone()
                                .Register(c => "from clone");

            var obj = clone.Resolve<HasTwoParamCtor<string, IAmAnInterface>>();

            obj.Value1.Should().Be("from clone");
            obj.Value2.Should().BeOfType<HasOneParamCtor<string>>();
            ((HasOneParamCtor<string>) obj.Value2).Value1.Should().Be("from clone");
        }

        [Fact]
        public void Registrations_on_the_clone_override_original_when_a_type_is_registered_in_both()
        {
            var original = new PocketContainer()
                .Register(c => "from original");
            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<string>().Should().Be("from clone");
        }

        [Fact]
        public void Strategies_are_cloned()
        {
            var original = new PocketContainer()
                .Register(c => new List<string>())
                .RegisterGeneric(typeof(IEnumerable<>), to: typeof(List<>));
      
            var clone = original.Clone();

            clone.Resolve<IEnumerable<string>>()
                 .Should()
                 .NotBeNull();
        }

        [Fact]
        public void When_a_strategy_on_the_clone_results_in_a_new_registration_then_the_original_contanier_is_not_affected()
        {
            var original = new PocketContainer();
            var clone = original
                .Register(c => new List<string>())
                .Clone()
                .RegisterGeneric(typeof(IEnumerable<>), to: typeof(List<>));

            clone.Resolve<IEnumerable<string>>();

            original.Count().Should().Be(clone.Count() - 1);
        }

        [Fact]
        public void A_strategy_on_the_clone_is_not_added_to_the_original()
        {
            var original = new PocketContainer();
            original.Clone()
                    .RegisterGeneric(typeof(IEnumerable<>), to: typeof(List<>));

            Action resolveFromOriginal = () =>
                original.Resolve<IEnumerable<string>>();

            resolveFromOriginal.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void When_a_singleton_registered_in_the_original_is_resolved_first_from_the_original_and_then_from_the_clone_it_is_the_same_instance()
        {
            var original = new PocketContainer();

            original.RegisterSingle(c => new object());

            var clone = original.Clone();

            var resolvedFromOriginal = original.Resolve<object>();
            var resolvedFromClone = clone.Resolve<object>();

            resolvedFromClone.Should().BeSameAs(resolvedFromOriginal);
        }

        [Fact]
        public void When_a_singleton_registered_in_the_original_is_resolved_first_from_the_clone_and_then_from_the_original_it_is_the_same_instance()
        {
            var original = new PocketContainer();

            original.RegisterSingle(c => new object());

            var clone = original.Clone();

            var resolvedFromClone = clone.Resolve<object>();
            var resolvedFromOriginal = original.Resolve<object>();

            resolvedFromOriginal.Should().BeSameAs(resolvedFromClone);
        }

        [Fact]
        public void When_RegisterSingle_is_called_on_the_clone_it_does_not_register_the_singleton_in_the_original()
        {
            var original = new PocketContainer();
            original.RegisterSingle(c => new object());

            var clone = original.Clone();
            clone.RegisterSingle(c => new object());

            var resolvedFromClone = clone.Resolve<object>();
            var resolvedFromOriginal = original.Resolve<object>();

            resolvedFromOriginal.Should()
                                .NotBeSameAs(resolvedFromClone);
        }

        [Fact]
        public void When_a_singleton_is_resolved_from_a_clone_first_and_original_second_it_is_the_same_instance()
        {
            var original = new PocketContainer();
            original.RegisterSingle(c => new object());

            var clone = original.Clone();

            var resolvedFromClone = clone.Resolve<object>();
            var resolvedFromOriginal = original.Resolve<object>();

            resolvedFromOriginal.Should()
                                .BeSameAs(resolvedFromClone);
        }
    }
}
