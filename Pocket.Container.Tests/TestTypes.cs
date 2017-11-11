using System;

namespace Pocket.Container.Tests
{
    public class HasOneParamCtor<T> : IAmAnInterface
    {
        public HasOneParamCtor(T value1)
        {
            Value1 = value1;
        }

        public T Value1 { get; }
    }

    public class HasTwoParamCtor<T1, T2> : HasOneParamCtor<T1>
    {
        public HasTwoParamCtor(T1 value1, T2 value2) : base(value1)
        {
            if (value2 == null)
            {
                throw new ArgumentNullException(nameof(value2));
            }
            Value2 = value2;
        }

        public T2 Value2 { get; }
    }

    public class HasOneOptionalParameterCtor<T>
    {
        public HasOneOptionalParameterCtor(T value = default(T))
        {
            Value = value;
        }

        public T Value { get; }
    }

    public class HasOneRequiredAndOneOptionalWithDefaultParamCtor<T>
    {
        public const int DefaultIntValue = 123;

        public HasOneRequiredAndOneOptionalWithDefaultParamCtor(T nonOptionalValue, int optionalIntValue = DefaultIntValue)
        {
            if (nonOptionalValue == null)
            {
                throw new ArgumentNullException(nameof(nonOptionalValue));
            }

            NonOptionalValue = nonOptionalValue;

            OptionalIntValue = optionalIntValue;
        }

        public T NonOptionalValue { get; }

        public int OptionalIntValue { get; }
    }

    public class HasOneNullableOptionalParamWithDefaultCtor
    {
        public const int DefaultIntValue = 123;

        public HasOneNullableOptionalParamWithDefaultCtor(int? optionalIntValue = DefaultIntValue)
        {
            OptionalIntValue = optionalIntValue;
        }

        public int? OptionalIntValue { get; }
    }

    public class HasOneOptionalAndOneRequiredParameterCtor<T1, T2>
    {
        public HasOneOptionalAndOneRequiredParameterCtor(T1 value1, T2 value2 = default(T2))
        {
            if (value1 == null)
            {
                throw new ArgumentNullException(nameof(value1));
            }

            Value1 = value1;
            Value2 = value2;
        }

        public T1 Value1 { get; }
        public T2 Value2 { get; }
    }

    public class HasTwoCtorsWithTheSameNumberOfParams
    {
        public HasTwoCtorsWithTheSameNumberOfParams(string one, string two)
        {
        }

        public HasTwoCtorsWithTheSameNumberOfParams(string one, int two)
        {
        }
    }

    public interface IAmAnInterface
    {
    }

    public interface IAmAGenericInterface<T>
    {
    }

    public class IAmAGenericImplementation<T> : IAmAGenericInterface<T>
    {
    }

    public class HasDefaultCtor : IAmAnInterface
    {
    }

    public class AlsoHasDefaultCtor : IAmAnInterface
    {
    }

    public class HasDefaultCtor<T> : HasDefaultCtor
    {
        public T Value { get; set; }
    }

    public class HasDefaultAndOneParamCtor<T> : HasDefaultCtor
    {
        public HasDefaultAndOneParamCtor()
        {
        }

        public HasDefaultAndOneParamCtor(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
