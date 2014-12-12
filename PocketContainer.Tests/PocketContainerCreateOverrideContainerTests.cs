// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerCreateOverrideContainerTests
    {
        [Test]
        public void Instances_are_resolved_from_the_primary_container_when_the_override_has_no_registration()
        {
            var primary = new PocketContainer()
                .Register(c => "from primary");

            var @override = primary.CreateOverrideContainer();

            @override.Resolve<string>()
                     .Should()
                     .Be("from primary");
        }

        [Test]
        public void Instances_are_resolved_from_the_override_when_it_has_a_registration()
        {
            var primary = new PocketContainer();

            var @override = primary.CreateOverrideContainer()
                                   .Register(c => "from override");

            @override.Resolve<string>().Should().Be("from override");
        }

        [Test]
        public void Dependencies_are_resolved_from_the_primary_container_when_the_override_has_no_registration()
        {
            var primary = new PocketContainer()
                .Register(c => "from primary");

            var @override = primary.CreateOverrideContainer();

            @override.Resolve<HasOneParamCtor<string>>()
                     .Value1
                     .Should()
                     .Be("from primary");
        }

        [Test]
        public void Dependencies_are_resolved_from_the_primary_container_when_registered_after_the_override_was_created()
        {
            var primary = new PocketContainer();

            var @override = primary.CreateOverrideContainer();
            primary.Register(c => "from primary");

            @override.Resolve<HasOneParamCtor<string>>()
                     .Value1
                     .Should()
                     .Be("from primary");
        }

        [Test]
        public void Dependencies_of_a_registration_from_the_primary_can_be_resolved_from_the_override()
        {
            var primary = new PocketContainer()
                .Register<IAmAnInterface>(c => c.Resolve<HasOneParamCtor<string>>());

            var @override = primary.CreateOverrideContainer()
                                   .Register(c => "from override");

            var obj = @override.Resolve<HasTwoParamCtor<string, IAmAnInterface>>();

            obj.Value1.Should().Be("from override");
            obj.Value2.Should().BeOfType<HasOneParamCtor<string>>();
            ((HasOneParamCtor<string>) obj.Value2).Value1.Should().Be("from override");
        }

        [Test]
        public void Lazy_registrations_in_the_override_do_not_modify_the_parent()
        {
            var primary = new PocketContainer();

            var @override = primary.CreateOverrideContainer();
            primary.Register(c => "from primary");

            @override.Resolve<HasDefaultCtor>();

            primary.Count(reg => reg.Key == typeof (HasDefaultCtor)).Should().Be(0);
        }

        [Test]
        public void Dependencies_are_resolved_from_the_override_when_it_has_a_registration()
        {
            var primary = new PocketContainer();

            var @override = primary.CreateOverrideContainer().Register(c => "from override");

            @override.Resolve<HasOneParamCtor<string>>()
                     .Value1
                     .Should()
                     .Be("from override");
        }

        [Test]
        public void Registrations_on_the_override_override_primary_when_a_type_is_registered_in_both()
        {
            var primary = new PocketContainer()
                .Register(c => "from primary");
            var @override = primary.CreateOverrideContainer().Register(c => "from override");

            @override.Resolve<string>().Should().Be("from override");
        }

        [Test]
        public void Strategies_are_cloned()
        {
            var primary = new PocketContainer().AutoMockInterfacesAndAbstractClasses();

            var @override = primary.CreateOverrideContainer();

            @override.Resolve<IEnumerable<string>>()
                     .Should()
                     .NotBeNull();
        }

        [Test]
        public void Strategies_added_to_the_override_do_not_modify_the_primary()
        {
            var primary = new PocketContainer();
            var @override = primary.CreateOverrideContainer()
                                   .AutoMockInterfacesAndAbstractClasses();

            Action resolve = () => primary.Resolve<IEnumerable<string>>();

            resolve.ShouldThrow<ArgumentException>();
        }
    }
}