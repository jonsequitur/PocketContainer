// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Its.Configuration;
using Microsoft.Its.Configuration;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerItsConfigurationSettingsStrategyTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.For<PocketContainerStrategyTestSettings>.GetSerializedSetting = key => @"{""Value"":""hello""}";
        }

        [Test]
        public void Types_with_names_ending_in_Settings_are_resolved_from_Settings_Get()
        {
            var container = new PocketContainer()
                .UseItsConfigurationForSettings();

            var settings = container.Resolve<PocketContainerStrategyTestSettings>();

            settings.Value.Should().Be("hello");
        }
    }

    public class PocketContainerStrategyTestSettings
    {
        public string Value { get; set; }
    }
}