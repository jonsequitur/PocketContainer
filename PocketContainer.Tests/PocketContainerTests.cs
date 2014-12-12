// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerTests
    {
        [Test]
        public void Can_resolve_unregistered_concrete_type_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container.Resolve<HasDefaultCtor>();

            result.Should().NotBeNull();
        }
        
        [Test]
        public void Can_resolve_Func_of_unregistered_concrete_type_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container.Resolve<Func<HasDefaultCtor>>();

            result().Should().NotBeNull();
        }
        
        [Test]
        public void Can_resolve_Func_of_unregistered_concrete_type_with_dependencies_when_it_has_only_a_default_ctor()
        {
            var container = new PocketContainer();

            var result = container
                .Register(c => "hello")
                .Resolve<Func<HasOneParamCtor<string>>>();

            result().Value1.Should().Be("hello");
        }

        [Test]
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

        [Test]
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

        [Test]
        public void Can_resolve_type_not_known_at_compile_time()
        {
            var container = new PocketContainer();
            container.Register(c => "hello");
            container.Register(c => new HasOneParamCtor<int>(42));

            var result = container.Resolve(typeof (HasTwoParamCtor<string, HasOneParamCtor<int>>)) as HasTwoParamCtor<string, HasOneParamCtor<int>>;

            result.Should().NotBeNull();

            result.Value1.Should().Be("hello");
            result.Value2.Value1.Should().Be(42);
        }

        [Test]
        public void Can_resolve_via_a_registered_factory_function()
        {
            var container = new PocketContainer().Register(c => "hello");

            container.Resolve<string>().Should().Be("hello");
        }

        [Test]
        public void Can_resolve_via_a_registered_untyped_factory_function()
        {
            var s = Guid.NewGuid().ToString();
            var container = new PocketContainer().Register(typeof (string), c => s);
            container.Resolve<string>().Should().Be(s);
        }

        [Test]
        public void When_the_same_type_is_registered_multiple_times_then_the_last_register_wins()
        {
            var container = new PocketContainer();

            container.Register(c => 1);
            container.Register(c => 2);

            container.Resolve<int>().Should().Be(2);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void Func_is_implicitly_registered_when_registering_a_type()
        {
            var container = new PocketContainer();

            container.Register(c => "oh hai");

            var obj = container.Resolve<HasOneParamCtor<Func<string>>>();

            obj.Value1().Should().Be("oh hai");
        }

        [Test]
        public void Func_is_resolvable_for_unregistered_types()
        {
            var container = new PocketContainer();

            container.Register(c => 123);

            var obj = container.Resolve<HasOneParamCtor<Func<int>>>();

            obj.Value1().Should().Be(123);
        }

        [Test]
        public void Lazy_is_implicitly_registered_when_registering_a_type()
        {
            var container = new PocketContainer();

            container.Register(c => "oh hai");

            var obj = container.Resolve<HasOneParamCtor<Lazy<string>>>();

            obj.Value1.Value.Should().Be("oh hai");
        }

        [Test]
        public void Lazy_is_resolvable_for_unregistered_types()
        {
            var container = new PocketContainer();

            container.Register(c => 123);

            var obj = container.Resolve<HasOneParamCtor<Lazy<int>>>();

            obj.Value1.Value.Should().Be(123);
        }

        [Test]
        public void PocketContainer_is_implicitly_registered_to_itself()
        {
            var container = new PocketContainer();

            var resolvedContainer = container.Resolve<PocketContainer>();

            Assert.AreSame(container, resolvedContainer);
        }

        [Test]
        public void Container_registrations_can_be_iterated()
        {
            var container = new PocketContainer();

            container.Count().Should().BeGreaterThan(1);
            container.Select(r => r.Key).Should().Contain(typeof (PocketContainer));
        }

        [Test]
        public void Default_construction_strategy_can_be_overridden_example_1_implicit_IEnumerable_support()
        {
            var container = new PocketContainer()
                .Register(c => "good day!")
                .Register(c => 456);

            // when someone asks for an IEnumerable<T> give back a List<T> containing the registered value 
            container.AddStrategy(type =>
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                {
                    return c =>
                    {
                        var singleInstanceType = type.GetGenericArguments().Single();
                        var listType = typeof (List<>).MakeGenericType(singleInstanceType);
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

        [Test]
        public void Default_construction_strategy_can_be_overridden_example_2_auto_mocking()
        {
            var container = new PocketContainer();

            // when someone asks for an IEnumerable<T> give back a List<T> containing the registered value 
            container.AddStrategy(type =>
            {
                if (type.IsInterface)
                {
                    return c =>
                    {
                        dynamic mock = Activator.CreateInstance(typeof (Mock<>).MakeGenericType(type));
                        return mock.Object;
                    };
                }
                return null;
            });

            var obj = container.Resolve<IObservable<string>>();

            obj.Should().NotBeNull();
        }

        [Test]
        public void When_default_construction_strategy_is_overridden_but_returns_null_then_it_falls_back_to_the_default()
        {
            var container = new PocketContainer()
                .Register(c => "still here");

            // when someone asks for an IEnumerable<T> give back a List<T> containing the registered value 
            container.AddStrategy(type =>
            {
                if (type.IsInterface)
                {
                    return c =>
                    {
                        dynamic mock = Activator.CreateInstance(typeof (Mock<>).MakeGenericType(type));
                        return mock.Object;
                    };
                }
                return null;
            });

            var obj = container.Resolve<HasOneParamCtor<string>>();

            obj.Should().NotBeNull();
            obj.Value1.Should().Be("still here");
        }

        [Test]
        public void Strategies_can_be_chained()
        {
            var container = new PocketContainer()
                .AddStrategy(t =>
                {
                    if (t == typeof (IEnumerable<int>))
                    {
                        return c => new[] { 1, 2, 3 };
                    }
                    return null;
                })
                .AddStrategy(t =>
                {
                    if (t == typeof (IEnumerable<string>))
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

        [Test]
        public void Explicitly_registered_instances_are_not_overwritten_by_overriding_the_default_construction_strategy()
        {
            var ints = Observable.Return(123);
            var container = new PocketContainer()
                .Register(c => ints);

            // when someone asks for an unregistered interface, generate a mock
            container.AddStrategy(type => c =>
            {
                if (type.IsInterface)
                {
                    dynamic mock = Activator.CreateInstance(typeof (Mock<>).MakeGenericType(type));
                    return mock.Object;
                }
                return null;
            });

            container.Resolve<IObservable<string>>().Should().NotBeNull();
            container.Resolve<IObservable<int>>().Should().BeSameAs(ints);
        }

        [Test]
        public void By_default_the_last_strategy_added_is_the_first_to_be_called()
        {
            var container = new PocketContainer()
                .AddStrategy(t => t == typeof (string)
                                      ? c => "first"
                                      : (Func<PocketContainer, object>) null)
                .AddStrategy(t => t == typeof (string)
                                      ? c => "second"
                                      : (Func<PocketContainer, object>) null);

            var s = container.Resolve<string>();

            s.Should().Be("second");
        }

        [Test]
        public void A_strategy_can_be_added_and_specified_to_be_called_after_existing_strategies()
        {
            var container = new PocketContainer()
                .AddStrategy(t => t == typeof (IList<string>)
                                      ? c => new[] { "first" }.ToList()
                                      : (Func<PocketContainer, object>) null)
                // adding the less specific strategy second
                .AddStrategy(t => t == typeof (IEnumerable<string>)
                                      ? c => new[] { "second" }
                                      : (Func<PocketContainer, object>) null,
                             executeFirst: false);

            container.Resolve<IEnumerable<string>>().Should().Contain("second");
            container.Resolve<IList<string>>().Should().Contain("first");
        }

        [Test]
        public void RegisterSingle_can_be_used_to_register_the_same_instance_for_the_lifetime_of_the_container()
        {
            var container = new PocketContainer()
                .RegisterSingle(c => new HasDefaultCtor());

            var one = container.Resolve<HasDefaultCtor>();
            var two = container.Resolve<HasDefaultCtor>();

            one.Should().BeSameAs(two);
        }

        [Test]
        public void RegisterSingle_non_generic_can_be_used_to_register_the_same_instance_for_the_lifetime_of_the_container()
        {
            var container = new PocketContainer()
                .RegisterSingle(typeof (HasOneParamCtor<int>), c => new HasOneParamCtor<int>(new Random().Next()));

            var one = container.Resolve<HasOneParamCtor<int>>();
            var two = container.Resolve<HasOneParamCtor<int>>();

            one.Should().BeSameAs(two);
            one.Value1.Should().Be(two.Value1);
        }

        [Test]
        public void RegisterSingle_can_overwrite_previous_RegisterSingle()
        {
            var container = new PocketContainer();

            container.RegisterSingle<IAmAnInterface>(c => new HasDefaultCtor());

            container.Resolve<IAmAnInterface>().Should().BeOfType<HasDefaultCtor>();

            container.RegisterSingle<IAmAnInterface>(c => new HasOneParamCtor<string>("second registration"));

            container.Resolve<IAmAnInterface>().Should().BeOfType<HasOneParamCtor<string>>();
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