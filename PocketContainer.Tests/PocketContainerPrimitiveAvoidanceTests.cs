// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerPrimitiveAvoidanceTests
    {
        [Test]
        public void Empty_constructor_is_chosen_over_one_containing_a_string()
        {
            var container = new PocketContainer().AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<string>>();

            obj.Should().NotBeNull();
            obj.Value.Should().BeNull();
        }

        [Test]
        public void Empty_constructor_is_chosen_over_one_containing_an_int()
        {
            var container = new PocketContainer().AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<int>>();

            obj.Should().NotBeNull();
            obj.Value.Should().Be(0);
        }

        [Test]
        public void Empty_constructor_is_chosen_over_one_containing_a_DateTime()
        {
            var container = new PocketContainer().AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<DateTime>>();

            obj.Should().NotBeNull();
            obj.Value.Should().Be(new DateTime());
        }

        [Test]
        public void Empty_constructor_is_chosen_over_one_containing_a_DateTimeOffset()
        {
            var container = new PocketContainer().AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<DateTimeOffset>>();

            obj.Should().NotBeNull();
            obj.Value.Should().Be(new DateTimeOffset());
        }

        [Test]
        public void Empty_constructor_is_chosen_over_one_containing_a_bool()
        {
            var container = new PocketContainer().AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<bool>>();

            obj.Should().NotBeNull();
            obj.Value.Should().BeFalse();
        }

        [Test]
        public void Non_empty_constructor_is_chosen_if_it_is_not_primitive()
        {
            var container = new PocketContainer()
                .AvoidConstructorsWithPrimitiveTypes();

            var obj = container
                .Resolve<HasDefaultAndOneParamCtor<HasDefaultCtor>>();

            obj.Should().NotBeNull();
            obj.Value.Should().NotBeNull();
        }

        [Test]
        public void A_constructor_with_a_primitive_is_chosen_if_no_other_is_available()
        {
            var container = new PocketContainer()
                .AvoidConstructorsWithPrimitiveTypes()
                .Register(c => "howdy");

            var obj = container
                .Resolve<HasTwoParamCtor<string, HasDefaultCtor>>();

            obj.Should().NotBeNull();
            obj.Value1.Should().Be("howdy");
        }
    }
}