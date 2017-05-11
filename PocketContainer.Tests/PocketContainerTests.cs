// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Pocket.Tests
{
    public class PocketContainerTests
    {
        [Fact]
        public void Can_resolve_unregistered_concrete_type_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container.Resolve<HasDefaultCtor>();

            result.Should().NotBeNull();
        }

        [Fact]
        public void Can_resolve_Func_of_unregistered_concrete_type_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container.Resolve<Func<HasDefaultCtor>>();

            result().Should().NotBeNull();
        }

        [Fact]
        public void Can_resolve_Func_of_unregistered_concrete_type_with_dependencies_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container
                .Register(c => "hello")
                .Resolve<Func<HasOneParamCtor<string>>>();

            result().Value1.Should().Be("hello");
        }

        [Fact]
        public void Can_resolve_unregistered_concrete_type_by_choosing_the_most_verbose_ctor_and_resolving_recursively()
        {
            var container = new PocketContainer();
            container.Register(c => "hello");
            container.Register(c => new HasOneParamCtor<string>("there"));

            var result = container.Resolve<HasTwoParamCtor<string, HasOneParamCtor<string>>>();

            result.Should().NotBeNull();
            result.Value1.Should().Be("hello");
            result.Value2.Value1.Should().Be("there");
        }

        [Fact]
        public void Can_resolve_Func_of_unregistered_concrete_type_by_choosing_the_most_verbose_ctor_and_resolving_recursively()
        {
            var container = new PocketContainer();
            container.Register(c => "hello");
            container.Register(c => new HasOneParamCtor<string>("there"));

            var result = container.Resolve<Func<HasTwoParamCtor<string, HasOneParamCtor<string>>>>();

            result().Should().NotBeNull();
            result().Value1.Should().Be("hello");
            result().Value2.Value1.Should().Be("there");
        }

        [Fact]
        public void Can_resolve_type_not_known_at_compile_time()
        {
            var container = new PocketContainer();
            container.Register(c => "hello");
            container.Register(c => new HasOneParamCtor<int>(42));

            var result = container.Resolve(typeof(HasTwoParamCtor<string, HasOneParamCtor<int>>)) as HasTwoParamCtor<string, HasOneParamCtor<int>>;

            result.Should().NotBeNull();

            result.Value1.Should().Be("hello");
            result.Value2.Value1.Should().Be(42);
        }

        [Fact]
        public void Can_resolve_via_a_registered_factory_function()
        {
            var container = new PocketContainer().Register(c => "hello");

            container.Resolve<string>().Should().Be("hello");
        }

        [Fact]
        public void Can_resolve_via_a_registered_untyped_factory_function()
        {
            var s = Guid.NewGuid().ToString();
            var container = new PocketContainer().Register(typeof(string), c => s);
            container.Resolve<string>().Should().Be(s);
        }

        public delegate void SomeDelegateType();

        [Fact]
        public void Cannot_resolve_an_unregistered_delegate_type()
        {
            var container = new PocketContainer();

            Action resolve = () => container.Resolve<SomeDelegateType>();

            resolve.ShouldThrow<ArgumentException>()
                   .And
                   .Message
                   .Should()
                   .Contain("SomeDelegateType");
        }

        [Fact]
        public void Can_resolve_a_registered_delegate_type()
        {
            var container = new PocketContainer();

            var f = new SomeDelegateType(Can_resolve_a_registered_delegate_type);

            container.Register(c => f);

            container.Resolve<SomeDelegateType>().Should().Be(f);
        }

        [Fact]
        public void When_the_same_type_is_registered_multiple_times_then_the_last_register_wins()
        {
            var container = new PocketContainer();

            container.Register(c => 1);
            container.Register(c => 2);

            container.Resolve<int>().Should().Be(2);
        }

        [Fact]
        public void When_two_ctors_of_the_same_length_are_found_then_a_useful_exception_is_thrown()
        {
            var container = new PocketContainer();

            Action resolve = () =>
                container.Resolve<HasTwoCtorsWithTheSameNumberOfParams>();

            resolve.ShouldThrow<ArgumentException>()
                   .And
                   .Message
                   .Should()
                   .Contain("HasTwoCtorsWithTheSameNumberOfParams");
        }

        [Fact]
        public void When_an_unregistered_interface_is_resolved_then_a_useful_exception_is_thrown()
        {
            var container = new PocketContainer();

            Action resolve = () =>
                container.Resolve<IAmAnInterface>();

            resolve.ShouldThrow<ArgumentException>()
                   .And
                   .Message
                   .Should()
                   .Contain("IAmAnInterface");
        }

        [Fact]
        public void Func_is_implicitly_registered_when_registering_a_type()
        {
            var container = new PocketContainer();

            container.Register(c => "oh hai");

            var obj = container.Resolve<HasOneParamCtor<Func<string>>>();

            obj.Value1().Should().Be("oh hai");
        }

        [Fact]
        public void Func_is_resolvable_for_unregistered_types()
        {
            var container = new PocketContainer();

            container.Register(c => 123);

            var obj = container.Resolve<HasOneParamCtor<Func<int>>>();

            obj.Value1().Should().Be(123);
        }

        [Fact]
        public void Lazy_is_implicitly_registered_when_registering_a_type()
        {
            var container = new PocketContainer();

            container.Register(c => "oh hai");

            var obj = container.Resolve<HasOneParamCtor<Lazy<string>>>();

            obj.Value1.Value.Should().Be("oh hai");
        }

        [Fact]
        public void Lazy_is_resolvable_for_unregistered_types()
        {
            var container = new PocketContainer();

            container.Register(c => 123);

            var obj = container.Resolve<HasOneParamCtor<Lazy<int>>>();

            obj.Value1.Value.Should().Be(123);
        }

        [Fact]
        public void PocketContainer_is_implicitly_registered_to_itself()
        {
            var container = new PocketContainer();

            var resolvedContainer = container.Resolve<PocketContainer>();

            container.Should().BeSameAs(resolvedContainer);
        }

        [Fact]
        public void Container_registrations_can_be_iterated()
        {
            var container = new PocketContainer();

            container.Count().Should().BeGreaterThan(1);
            container.Select(r => r.Key).Should().Contain(typeof(PocketContainer));
        }

        [Fact]
        public void Default_construction_strategy_can_be_overridden_example_1_implicit_IEnumerable_support()
        {
            var container = new PocketContainer()
                .Register(c => "good day!")
                .Register(c => 456);

            // when someone asks for an IEnumerable<T> give back a List<T> containing the registered value 
            container.AddStrategy(type =>
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var singleInstanceType = type.GetGenericArguments().Single();
                    var listType = typeof(List<>).MakeGenericType(singleInstanceType);

                    return c =>
                    {
                        var list = Activator.CreateInstance(listType);
                        var resolved = c.Resolve(singleInstanceType);
                        ((dynamic) list).Add((dynamic) resolved);
                        return list;
                    };
                }
                return null;
            });

            var strings = container.Resolve<IEnumerable<string>>();
            var ints = container.Resolve<IEnumerable<int>>();

            Console.WriteLine(strings);
            Console.WriteLine(ints);

            ints.Single().Should().Be(456);
            strings.Single().Should().Be("good day!");
        }

        [Fact]
        public void When_an_added_strategy_returns_null_then_it_falls_back_to_the_default()
        {
            var container = new PocketContainer()
                .Register(c => "still here");

            // when someone asks for an IEnumerable<T> give back a List<T> containing the registered value 
            container.AddStrategy(type =>
            {
                if (type.GetTypeInfo().IsInterface)
                {
                    return c =>
                    {
                        dynamic mock = Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
                        return mock.Object;
                    };
                }
                return null;
            });

            var obj = container.Resolve<HasOneParamCtor<string>>();

            obj.Should().NotBeNull();
            obj.Value1.Should().Be("still here");
        }

        [Fact]
        public void A_strategy_can_be_used_to_make_PocketContainer_return_null_rather_than_throw()
        {
            var container = new PocketContainer()
                .AddStrategy(t => c => null);

            container.Resolve<IList<string>>()
                     .Should()
                     .BeNull();
        }

        [Fact]
        public void Strategies_can_be_chained()
        {
            var container = new PocketContainer()
                .AddStrategy(t =>
                {
                    if (t == typeof(IEnumerable<int>))
                    {
                        return c => new[] { 1, 2, 3 };
                    }
                    return null;
                })
                .AddStrategy(t =>
                {
                    if (t == typeof(IEnumerable<string>))
                    {
                        return c => new[] { "1", "2", "3" };
                    }
                    return null;
                });

            container.Resolve<IEnumerable<int>>()
                     .Should()
                     .BeEquivalentTo(new[] { 1, 2, 3 });
            container.Resolve<IEnumerable<string>>()
                     .Should()
                     .BeEquivalentTo("1", "2", "3");
        }

        [Fact]
        public void Explicitly_registered_instances_are_not_overwritten_by_overriding_the_default_construction_strategy()
        {
            var ints = Enumerable.Range(1, 123);
            var container = new PocketContainer()
                .Register(c => ints);

            // when someone asks for an unregistered IEnumerable, return a list
            container.AddStrategy(type => c =>
            {
                if (type.GetTypeInfo().IsInterface)
                {
                    return Activator.CreateInstance(typeof(List<>)
                                                        .MakeGenericType(
                                                            type.GetTypeInfo()
                                                                .GenericTypeArguments
                                                                .Single()));
                }
                return null;
            });

            container.Resolve<IEnumerable<string>>().Should().NotBeNull();
            container.Resolve<IEnumerable<int>>().Should().BeSameAs(ints);
        }

        [Fact]
        public void By_default_the_last_strategy_added_is_the_first_to_be_called()
        {
            var container = new PocketContainer()
                .AddStrategy(t => t == typeof(string)
                                      ? c => "first"
                                      : (Func<PocketContainer, object>) null)
                .AddStrategy(t => t == typeof(string)
                                      ? c => "second"
                                      : (Func<PocketContainer, object>) null);

            var s = container.Resolve<string>();

            s.Should().Be("second");
        }

        [Fact]
        public void A_strategy_can_be_added_and_specified_to_be_called_after_existing_strategies()
        {
            var container = new PocketContainer()
                .AddStrategy(t => t == typeof(IList<string>)
                                      ? c => new[] { "first" }.ToList()
                                      : (Func<PocketContainer, object>) null)
                // adding the less specific strategy second
                .AddStrategy(t => t == typeof(IEnumerable<string>)
                                      ? c => new[] { "second" }
                                      : (Func<PocketContainer, object>) null,
                             executeFirst: false);

            container.Resolve<IEnumerable<string>>().Should().Contain("second");
            container.Resolve<IList<string>>().Should().Contain("first");
        }

        [Fact]
        public void RegisterSingle_can_be_used_to_register_the_same_instance_for_the_lifetime_of_the_container()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasDefaultCtor());

            var one = container.Resolve<HasDefaultCtor>();
            var two = container.Resolve<HasDefaultCtor>();

            one.Should().BeSameAs(two);
        }

        [Fact]
        public void RegisterSingle_returns_only_single_instances_when_dependent_and_dependency_are_both_registered_as_single()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasDefaultCtor())
                .RegisterSingle(c => new HasOneParamCtor<HasDefaultCtor>(c.Resolve<HasDefaultCtor>()));

            var one = container.Resolve<HasOneParamCtor<HasDefaultCtor>>();
            var two = container.Resolve<HasOneParamCtor<HasDefaultCtor>>();

            one.Should().BeSameAs(two);
            one.Value1.Should().BeSameAs(two.Value1);
        }

        [Fact]
        public void RegisterSingle_non_generic_can_be_used_to_register_the_same_instance_for_the_lifetime_of_the_container()
        {
            var container = new PocketContainer()
                .RegisterSingle(typeof(HasOneParamCtor<int>), c => new HasOneParamCtor<int>(new Random().Next()));

            var one = container.Resolve<HasOneParamCtor<int>>();
            var two = container.Resolve<HasOneParamCtor<int>>();

            one.Should().BeSameAs(two);
            one.Value1.Should().Be(two.Value1);
        }

        [Fact]
        public void RegisterSingle_can_overwrite_previous_RegisterSingle()
        {
            var container = new PocketContainer();

            container.RegisterSingle<IAmAnInterface>(c => new HasDefaultCtor());

            container.Resolve<IAmAnInterface>().Should().BeOfType<HasDefaultCtor>();

            container.RegisterSingle<IAmAnInterface>(c => new HasOneParamCtor<string>("second registration"));

            container.Resolve<IAmAnInterface>().Should().BeOfType<HasOneParamCtor<string>>();
        }

        [Fact]
        public void PocketContainer_can_throw_a_customized_exception_on_resolve_failure_via_generic_Resolve()
        {
            var container = new PocketContainer
            {
                OnFailedResolve = (type, ex) => new DataMisalignedException("Your data is completely out of alignment.", ex)
            };

            Action resolve = () => container.Resolve<IEnumerable<string>>();

            resolve.ShouldThrow<DataMisalignedException>();
        }

        [Fact]
        public void PocketContainer_can_throw_a_customized_exception_on_recursive_resolve_failure_via_non_generic_Resolve()
        {
            var container = new PocketContainer
            {
                OnFailedResolve = (type, ex) => new DataMisalignedException("Your data is completely out of alignment.", ex)
            };

            Action resolve = () => container.Resolve(typeof(IEnumerable<string>));

            resolve.ShouldThrow<DataMisalignedException>();
        }
    }

    public class HasOneParamCtor<T> : IAmAnInterface
    {
        public HasOneParamCtor(T value1)
        {
            if (value1 == null)
            {
                throw new ArgumentNullException("value1");
            }
            Value1 = value1;
        }

        public T Value1 { get; private set; }
    }

    public class HasTwoParamCtor<T1, T2> : HasOneParamCtor<T1>
    {
        public HasTwoParamCtor(T1 value1, T2 value2) : base(value1)
        {
            if (value2 == null)
            {
                throw new ArgumentNullException("value2");
            }
            Value2 = value2;
        }

        public T2 Value2 { get; private set; }
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

    public class HasDefaultCtor : IAmAnInterface
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
