// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerAutoMockingStrategyTests
    {
        [Test]
        public void When_an_unregistered_interface_is_requested_then_a_mock_is_returned()
        {
            var container = new PocketContainer()
                .AutoMockInterfacesAndAbstractClasses();

            var strings = container.Resolve<IEnumerable<string>>();

            strings.Should().NotBeNull();
        }

        [Test]
        public void When_an_unregistered_abstract_class_is_requested_then_a_mock_is_returned()
        {
            var container = new PocketContainer()
                .AutoMockInterfacesAndAbstractClasses();

            var obj = container.Resolve<PocketContainerSingleImplementationStrategyTests.AbstractlyImplements_IHaveOnlyOneImplementation>();

            obj.Should().NotBeNull();
        }
    }
}