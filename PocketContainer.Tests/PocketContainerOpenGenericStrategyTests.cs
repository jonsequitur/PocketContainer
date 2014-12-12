// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerOpenGenericStrategyTests
    {
        [Test]
        public void An_open_generic_interface_can_be_registered_to_an_open_generic_type_and_resolved_correctly()
        {
            var container = new PocketContainer();

            container
                .Register<List<string>>(c => new List<string>())
                .RegisterGeneric(variantsOf: typeof (IEnumerable<>), to: typeof (List<>));

            container.Resolve<IEnumerable<string>>().Should().BeOfType<List<string>>();
        }

        [Test]
        public void When_a_non_open_generic_type_is_registered_as_the_variantsOf_argument_then_it_throws()
        {
            var container = new PocketContainer();

            Action registerWrongType = () =>
                container.RegisterGeneric(typeof (string), typeof (List<>));

            registerWrongType.ShouldThrow<ArgumentException>().And.Message.Should().Contain("'variantsOf'");
        }

        [Test]
        public void When_a_non_open_generic_type_is_registered_as_the_to_argument_then_it_throws()
        {
            var container = new PocketContainer();

            Action registerWrongType = () =>
                container.RegisterGeneric(typeof (IEnumerable<>), typeof (string));

            registerWrongType.ShouldThrow<ArgumentException>().And.Message.Should().Contain("'to'");
        }
    }
}