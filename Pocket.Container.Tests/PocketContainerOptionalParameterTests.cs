using System;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests;

public class PocketContainerOptionalParameterTests
{
    [Fact]
    public void Optional_parameters_for_non_primitive_types_are_resolved_when_registered()
    {
        var container = new PocketContainer()
            .Register<IAmAnInterface>(c => new HasDefaultCtor());

        var o = container.Resolve<TakesOptionalParamWithDefaultValueSpecifiedAsDefault<IAmAnInterface>>();

        o.OptionalValue.Should().NotBeNull();
    }

    [Fact]
    public void Optional_parameters_for_non_primitive_types_are_passed_null_when_not_registered()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalParamWithDefaultValueSpecifiedAsDefault<IAmAnInterface>>();

        o.OptionalValue.Should().BeNull();
    }

    [Fact]
    public void Optional_struct_parameters_with_no_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalParamWithDefaultValueSpecifiedAsDefault<int>>();

        o.OptionalValue.Should().Be(0);
    }

    [Fact]
    public void Optional_nullable_struct_parameters_with_no_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalParamWithDefaultValueSpecifiedAsDefault<int?>>();

        o.OptionalValue.Should().Be(0);
    }

    [Fact]
    public void Optional_struct_parameters_with_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalIntParamWithDefaultValue>();

        o.OptionalValue.Should().Be(TakesOptionalIntParamWithDefaultValue.DefaultValue);
    }

    [Fact]
    public void Optional_nullable_struct_parameters_with_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalNullableIntParamWithDefaultValue>();

        o.OptionalValue.Should().Be(TakesOptionalNullableIntParamWithDefaultValue.DefaultValue);
    }

    [Fact]
    public void Optional_string_parameters_with_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalStringParamWithDefaultValue>();

        o.OptionalValue.Should().Be(TakesOptionalStringParamWithDefaultValue.DefaultValue);
    }

    [Fact]
    public void Optional_string_parameters_with_no_default_value_are_filled_correctly()
    {
        var container = new PocketContainer();

        var o = container.Resolve<TakesOptionalParamWithDefaultValueSpecifiedAsDefault<string>>();

        o.OptionalValue.Should().BeNull();
    }
}

public class TakesOptionalIntParamWithDefaultValue
{
    public const int DefaultValue = 123;

    public TakesOptionalIntParamWithDefaultValue(int optionalValue = DefaultValue)
    {
        OptionalValue = optionalValue;
    }

    public int OptionalValue { get; }
}

public class TakesOptionalNullableIntParamWithDefaultValue
{
    public const int DefaultValue = 123;

    public TakesOptionalNullableIntParamWithDefaultValue(int? optionalValue = DefaultValue)
    {
        OptionalValue = optionalValue;
    }

    public int? OptionalValue { get; }
}

public class TakesOptionalStringParamWithDefaultValue
{
    public const string DefaultValue = "the-default-value";

    public TakesOptionalStringParamWithDefaultValue(string optionalValue = DefaultValue)
    {
        OptionalValue = optionalValue;
    }

    public string OptionalValue { get; }
}

public class TakesOptionalParamWithDefaultValueSpecifiedAsDefault<T>
{
    public TakesOptionalParamWithDefaultValueSpecifiedAsDefault(T optionalValue = default(T))
    {
        OptionalValue = optionalValue;
    }

    public T OptionalValue { get; }
}