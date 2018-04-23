// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using FluentAssertions;
using Pocket.Container.Test.Assembly;
using PocketContainer;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerTestFakesStrategyTests
    {
        [Fact]
        public void When_looking_for_fakes_near_to_a_type_nested_class_is_preferred()
        {
            var container = new PocketContainer().ResolveWithFakeTypesCloseTo(this);

            var resolved = container.Resolve<RealObject>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<RealObjectNestedFake>();
        }

        [Fact]
        public void When_looking_for_fakes_near_to_a_type_the_closest_is_used()
        {
            var container = new PocketContainer().ResolveWithFakeTypesCloseTo<RealObject>();

            var resolved = container.Resolve<RealObject>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<RealObjectFake>();
        }

        [Fact]
        public void When_looking_for_fakes_in_a_specific_Assemly_closest_is_used()
        {
            var container = new PocketContainer().ResolveWithFakeTypesCloseTo<RealObject>();

            var resolved = container.Resolve<RealObject>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<RealObjectFake>();
        }

        [Fact]
        public void When_looking_for_fakes_user_can_speficy_convention()
        {
            var container = new PocketContainer().ResolveWithFakeTypesCloseTo<RealObject>("Surrogate");

            var resolved = container.Resolve<RealObject>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<RealObjectSurrogate>();
        }

        [Fact]
        public void When_looking_for_fakes_near_to_a_type_user_can_speficy_convention()
        {
            var container = new PocketContainer().ResolveWithFakeTypesCloseTo(this, "Surrogate");

            var resolved = container.Resolve<RealObject>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<RealObjectSurrogate>();
        }

        [Fact]
        public void When_looking_for_fakes_they_can_be_constriained_to_a_specific_assembly()
        {
            var container = new PocketContainer().ResolveWithFakeTypesFromAssembly(typeof(HttpClientrSurrogate).Assembly, "Surrogate");

            var resolved = container.Resolve<HttpClient>();
            resolved.Should().NotBeNull();
            resolved.Should().BeOfType<HttpClientrSurrogate>();
        }


        public class RealObjectNestedFake : RealObject
        {
            public RealObjectNestedFake() : base("Value for tests - NESTED")
            {
            }
        }

        public class NestedContainer
        {
            public class RealObjectNestedNestedFake : RealObject
            {
                public RealObjectNestedNestedFake() : base("Value for tests - NESTED TWICE")
                {
                }
            }
        }
    }

    public class RealObject
    {
        public string RequiredValue { get; }

        public RealObject(string requiredValue)
        {
            if (string.IsNullOrWhiteSpace(requiredValue))
            {
                throw new System.ArgumentException("message", nameof(requiredValue));
            }

            RequiredValue = requiredValue;
        }
    }

    public class RealObjectFake : RealObject
    {
        public RealObjectFake() : base("Value for tests")
        {
        }
    }

    public class RealObjectSurrogate : RealObject
    {
        public RealObjectSurrogate() : base("Value for tests")
        {
        }
    }
}