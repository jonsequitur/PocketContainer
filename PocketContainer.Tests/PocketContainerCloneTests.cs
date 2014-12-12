// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerCloneTests
    {
        [Test]
        public void Top_level_instances_are_resolved_from_the_original_container_when_the_clone_has_no_registration()
        {
            var original = new PocketContainer()
                .Register(c => "from original");

            var clone = original.Clone();

            clone.Resolve<string>()
                .Should()
                .Be("from original");
        }

        [Test]
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

        [Test]
        public void Top_level_instances_are_resolved_from_the_clone_when_it_has_a_registration()
        {
            var original = new PocketContainer();

            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<string>().Should().Be("from clone");
        }

        [Test]
        public void Dependencies_are_resolved_from_the_clone_when_it_has_a_registration()
        {
            var original = new PocketContainer();

            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<HasOneParamCtor<string>>()
                .Value1
                .Should()
                .Be("from clone");
        }

        [Test]
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

        [Test]
        public void Registrations_on_the_clone_override_original_when_a_type_is_registered_in_both()
        {
            var original = new PocketContainer()
                .Register(c => "from original");
            var clone = original.Clone().Register(c => "from clone");

            clone.Resolve<string>().Should().Be("from clone");
        }

        [Test]
        public void Strategies_are_cloned()
        {
            var original = new PocketContainer().AutoMockInterfacesAndAbstractClasses();

            var clone = original.Clone();

            clone.Resolve<IEnumerable<string>>()
                .Should()
                .NotBeNull();
        }

        [Test]
        public void When_a_strategy_on_the_clone_results_in_a_new_registration_then_the_original_contanier_is_not_affected()
        {
            var original = new PocketContainer();
            var clone = original.Clone().AutoMockInterfacesAndAbstractClasses();

            clone.Resolve<IEnumerable<string>>();

            original.Count().Should().Be(clone.Count() - 1);
        }

        [Test]
        public void A_strategy_on_the_clone_is_not_added_to_the_original()
        {
            var original = new PocketContainer();
            original.Clone().AutoMockInterfacesAndAbstractClasses();

            Action resolveFromOriginal = () =>
                original.Resolve<IEnumerable<string>>();

            resolveFromOriginal.ShouldThrow<ArgumentException>();
        }
    }
}