using System;
using FluentAssertions;
using Xunit;

namespace Pocket.TypeDiscovery.Tests
{
    public class TypeDiscoveryTests
    {
        [Fact]
        public void ConcreteTypes_contains_non_abstract_non_generic_classes()
        {
            Discover.ConcreteTypes().Should().Contain(typeof(ConcreteHandlerOfMultipleCommands));
        }

        [Fact]
        public void Concrete_types_does_not_include_interfaces()
        {
            Discover.ConcreteTypes().Should().NotContain(t => t.IsInterface);
        }

        [Fact]
        public void Concrete_types_does_not_include_abstract_classes()
        {
            Discover.ConcreteTypes().Should().NotContain(t => t.IsAbstract);
        }

        [Fact]
        public void Concrete_types_does_not_include_static_classes()
        {
            Discover.ConcreteTypes().Should().NotContain(t => t == typeof(StaticClass));
        }

        [Fact]
        public void Concrete_types_does_not_include_delegates()
        {
            Discover.ConcreteTypes().Should().NotContain(t => t == typeof(ADelegate));
        }

        [Fact]
        public void Concrete_types_does_not_include_enums()
        {
            Discover.ConcreteTypes().Should().NotContain(t => t == typeof(Enum));
        }

        [Fact]
        public void DerivedFrom_returns_classes_implementing_the_specified_interface()
        {
            Discover.ConcreteTypes()
                    .DerivedFrom(typeof(ICommand))
                    .Should()
                    .Contain(typeof(StringCommand));
        }

        [Fact]
        public void ImplementingOpenGenericInterfacess_returns_types_implementing_specified_open_generic_interfaces()
        {
            new[] { typeof(StringCommand) }
                .ImplementingOpenGenericInterfaces(typeof(ICommand<>))
                .Should()
                .Contain(typeof(StringCommand));
        }

        [Fact]
        public void ImplementingOpenGenericInterfacess_returns_types_inheriting_classes_implementing_specified_open_generic_interfaces()
        {
            var sourceTypes = new[] { typeof(ConcreteHandler) };

            var types = sourceTypes.ImplementingOpenGenericInterfaces(typeof(ICommandHandler<>));

            types.Should().Contain(typeof(ConcreteHandler));
        }

        public abstract class AbstractHandler : ICommandHandler<ICommand>
        {
        }

        public class ConcreteHandler : AbstractHandler
        {
        }

        public class ConcreteHandlerOfMultipleCommands :
            ICommandHandler<ICommand<string>>,
            ICommandHandler<ICommand<int>>
        {
        }

        public interface ICommand
        {
        }

        public class StringCommand : ICommand<string>
        {
        }

        public interface ICommand<T> : ICommand
        {
        }

        public interface ICommandHandler<T>
        {
        }

        public enum Enum
        {
        }

        public static class StaticClass
        {
        }

        public delegate void ADelegate();
    }
}
