using System;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerTryRegisterTests
    {
        [Fact]
        public void When_a_registration_already_exists_then_TryRegister_T_does_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.Register(c => "one");
            container.TryRegister(c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_no_registration_exists_then_TryRegister_T_registers()
        {
            var container = new PocketContainer();

            container.TryRegister(c => "one");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_strategy_has_resolved_an_unregistered_type_then_TryRegister_T_will_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.AddStrategy(type =>
            {
                if (typeof(string) == type)
                {
                    return c => "one";
                }

                return null;
            });

            // trigger the strategy
            container.Resolve<string>();

            container.TryRegister(c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_registration_already_exists_then_TryRegister_does_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.Register(c => "one");
            container.TryRegister(typeof(string), c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_no_registration_exists_then_TryRegister_registers()
        {
            var container = new PocketContainer();

            container.TryRegister(typeof(string), c => "one");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_strategy_has_resolved_an_unregistered_type_then_TryRegister_will_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.AddStrategy(type =>
            {
                if (typeof(string) == type)
                {
                    return c => "one";
                }

                return null;
            });

            // trigger the strategy
            container.Resolve<string>();

            container.TryRegister(typeof(string), c => "two");

            container.Resolve<string>().Should().Be("one");
        }


           [Fact]
        public void When_a_registration_already_exists_then_TryRegisterSingle_T_does_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.Register(c => "one");
            container.TryRegisterSingle(c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_no_registration_exists_then_TryRegisterSingle_T_registers()
        {
            var container = new PocketContainer();

            container.TryRegisterSingle(c => "one");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_strategy_has_resolved_an_unregistered_type_then_TryRegisterSingle_T_will_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.AddStrategy(type =>
            {
                if (typeof(string) == type)
                {
                    return c => "one";
                }

                return null;
            });

            // trigger the strategy
            container.Resolve<string>();

            container.TryRegisterSingle(c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_registration_already_exists_then_TryRegisterSingle_does_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.Register(c => "one");
            container.TryRegisterSingle(typeof(string), c => "two");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_no_registration_exists_then_TryRegisterSingle_registers()
        {
            var container = new PocketContainer();

            container.TryRegisterSingle(typeof(string), c => "one");

            container.Resolve<string>().Should().Be("one");
        }

        [Fact]
        public void When_a_strategy_has_resolved_an_unregistered_type_then_TryRegisterSingle_will_not_overwrite_it()
        {
            var container = new PocketContainer();

            container.AddStrategy(type =>
            {
                if (typeof(string) == type)
                {
                    return c => "one";
                }

                return null;
            });

            // trigger the strategy
            container.Resolve<string>();

            container.TryRegisterSingle(typeof(string), c => "two");

            container.Resolve<string>().Should().Be("one");
        }
    }
}
