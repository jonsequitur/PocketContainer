using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.CumulativeRegistration.Tests
{
    public class PocketContainerCumulativeRegistrationTests
    {
        [Fact]
        public void Multiple_registrations_of_T_can_be_resolved_as_an_IEnumerable_of_T()
        {
            var container = new PocketContainer()
                .AccumulateRegistrations();

            container.Register(c => "one");
            container.Register(c => "two");

            container.Resolve<IEnumerable<string>>()
                     .Should()
                     .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Resolve_doesnt_result_in_cumulative_registrations()
        {
            var container = new PocketContainer();

            container.AddStrategy(t =>
            {
                if (t == typeof(int))
                {
                    var counter = 0;
                    return c => ++counter;
                }
                return null;
            });

            var one = container.Resolve<int>();
            var two = container.Resolve<int>();

            Console.WriteLine(new { one, two });

            Action resolveEnumerable = () => container.Resolve<IEnumerable<int>>();

            resolveEnumerable.Should().Throw<ArgumentException>();
        }
    }
}
