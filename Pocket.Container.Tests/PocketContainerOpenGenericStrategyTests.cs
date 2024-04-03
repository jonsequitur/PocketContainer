// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests;

public class PocketContainerOpenGenericStrategyTests
{
    [Fact]
    public void An_open_generic_interface_can_be_registered_to_an_open_generic_type_and_resolved_correctly()
    {
        var container = new PocketContainer();

        container
            .RegisterGeneric(
                variantsOf: typeof(IAmAGenericInterface<>),
                to: typeof(IAmAGenericImplementation<>));

        container.Resolve<IAmAGenericInterface<string>>()
                 .Should()
                 .BeOfType<IAmAGenericImplementation<string>>();
    }

    [Fact]
    public void Open_generic_registrations_can_be_singletons()
    {
        var container = new PocketContainer();

        container
            .RegisterGeneric(
                variantsOf: typeof(IAmAGenericInterface<>),
                to: typeof(IAmAGenericImplementation<>),
                singletons: true);

        var resolvedOnce = container.Resolve<IAmAGenericInterface<int>>();
        var resolvedTwice = container.Resolve<IAmAGenericInterface<int>>();

        resolvedOnce
            .Should()
            .BeSameAs(resolvedTwice);
    }

    [Fact]
    public void When_a_non_open_generic_type_is_registered_as_the_variantsOf_argument_then_it_throws()
    {
        var container = new PocketContainer();

        Action registerWrongType = () =>
            container.RegisterGeneric(typeof(string), typeof(List<>));

        registerWrongType.Should().Throw<ArgumentException>()
                         .And
                         .Message
                         .Should()
                         .Contain("'variantsOf'");
    }

    [Fact]
    public void When_a_non_open_generic_type_is_registered_as_the_to_argument_then_it_throws()
    {
        var container = new PocketContainer();

        Action registerWrongType = () =>
            container.RegisterGeneric(typeof(IEnumerable<>), typeof(string));

        registerWrongType.Should().Throw<ArgumentException>()
                         .And
                         .Message
                         .Should()
                         .Contain("'to'");
    }
}