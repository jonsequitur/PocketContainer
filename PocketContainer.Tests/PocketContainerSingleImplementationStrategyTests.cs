using System;
using System.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerSingleImplementationStrategyTests
    {
        [Test]
        public void When_a_single_concrete_type_implementing_an_interface_is_found_then_it_is_Resolved()
        {
            var container = new PocketContainer()
                .IfOnlyOneImplementationUseIt();

            container.Resolve<IHaveOnlyOneImplementation>()
                     .Should()
                     .NotBeNull();
        }

        [Test]
        public void When_a_single_concrete_type_implementing_an_abstract_type_is_found_then_it_is_Resolved()
        {
            var container = new PocketContainer()
                .IfOnlyOneImplementationUseIt();

            container.Resolve<AbstractlyImplements_IHaveOnlyOneImplementation>()
                     .Should()
                     .NotBeNull();
        }

        [Test]
        public void When_a_concrete_type_is_requested_then_it_is_Resolved()
        {
            var container = new PocketContainer()
                .IfOnlyOneImplementationUseIt();

            container.Resolve<Implements_IHaveOnlyOneImplementation>()
                     .Should()
                     .NotBeNull();
        }

        [Test]
        public void When_multiple_implementations_are_found_then_an_error_is_thrown_on_Resolve()
        {
            var container = new PocketContainer()
                .IfOnlyOneImplementationUseIt();

            Action resolve = () => container.Resolve<IEnumerable>();
            resolve.ShouldThrow<ArgumentException>();
        }

        public interface IHaveOnlyOneImplementation
        {
        }

        public class Implements_IHaveOnlyOneImplementation : AbstractlyImplements_IHaveOnlyOneImplementation
        {
        }

        public abstract class AbstractlyImplements_IHaveOnlyOneImplementation : IHaveOnlyOneImplementation
        {
        }
    }
}