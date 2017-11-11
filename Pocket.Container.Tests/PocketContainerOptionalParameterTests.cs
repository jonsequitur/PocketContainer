using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerOptionalParameterTests
    {
        [Fact]
        public void Optional_struct_parameters_with_no_default_value_are_filled_correctly()
        {
            var container = new PocketContainer();

            var o = container.Resolve<TakesOptionalParamWithNoDefaultValue<int>>();

            o.OptionalValue.Should().Be(0);
        }

        [Fact]
        public void Optional_nullable_struct_parameters_with_no_default_value_are_filled_correctly()
        {
            var container = new PocketContainer();

            var o = container.Resolve<TakesOptionalParamWithNoDefaultValue<int?>>();

            o.OptionalValue.Should().Be(0);
        }

        [Fact]
        public void Optional_nullable_struct_parameters_with_default_value_are_filled_correctly()
        {
            var container = new PocketContainer();

            var o = container.Resolve<TakesOptionalNullableIntParamWithDefaultValue>();

            o.OptionalValue.Should().Be(TakesOptionalNullableIntParamWithDefaultValue.DefaultIntValue);
        }

        [Fact]
        public void Optional_string_parameters_with_default_value_are_filled_correctly()
        {
            var container = new PocketContainer();

            var o = container.Resolve<TakesOptionalStringParamWithDefaultValue>();

            o.OptionalValue.Should().Be(TakesOptionalStringParamWithDefaultValue.DefaultIntValue);
        }

        [Fact]
        public void Optional_string_parameters_with_no_default_value_are_filled_correctly()
        {
            var container = new PocketContainer();

            var o = container.Resolve<TakesOptionalParamWithNoDefaultValue<string>>();

            o.OptionalValue.Should().BeNull();
        }
    }

    public class TakesOptionalNullableIntParamWithDefaultValue
    {
        public const int DefaultIntValue = 123;

        public TakesOptionalNullableIntParamWithDefaultValue(int? optionalValue = DefaultIntValue)
        {
            OptionalValue = optionalValue;
        }

        public int? OptionalValue { get; }
    }

    public class TakesOptionalStringParamWithDefaultValue
    {
        public const string DefaultIntValue = "the-default-value";

        public TakesOptionalStringParamWithDefaultValue(string optionalValue = DefaultIntValue)
        {
            OptionalValue = optionalValue;
        }

        public string OptionalValue { get; }
    }

    public class TakesOptionalParamWithNoDefaultValue<T>
    {
        public TakesOptionalParamWithNoDefaultValue(T optionalValue = default(T))
        {
            OptionalValue = optionalValue;
        }

        public T OptionalValue { get; }
    }
}
