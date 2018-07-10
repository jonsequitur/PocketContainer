// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Strategy = System.Func<System.Type, System.Func<Pocket.PocketContainer, object>>;

#pragma warning disable CS0436 // Type conflicts with imported type

namespace Pocket
{
    /// <summary>
    /// An embedded dependency injection container, for when you want to use a container without adding an assembly dependency.
    /// </summary>
    /// <remarks>The default resolution strategy the following conventions: 
    /// * A concrete type can be resolved without explicit registration. 
    /// * PocketContainer will choose the longest constructor and resolve the types to satisfy its arguments. This continues recursively until the graph is built.
    /// * If it fails to build a dependency somewhere in the graph, an ArgumentException is thrown.</remarks>
#if !SourceProject
    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal partial class PocketContainer : IEnumerable<KeyValuePair<Type, Func<PocketContainer, object>>>
    {
        private static readonly MethodInfo resolveMethod =
            typeof(PocketContainer).GetMethod(nameof(Resolve), Type.EmptyTypes);

        private static readonly MethodInfo resolveOptionalMethod =
            typeof(PocketContainer).GetMethod(nameof(ResolveOptional), Type.EmptyTypes);

        private static readonly MethodInfo registerMethod =
            typeof(PocketContainer).GetMethods().Single(m => m.Name == nameof(Register) && m.IsGenericMethod);

        private static readonly MethodInfo registerSingleMethod =
            typeof(PocketContainer).GetMethods().Single(m => m.Name == nameof(RegisterSingle) && m.IsGenericMethod);

        private ConcurrentDictionary<Type, Func<PocketContainer, object>> resolvers = new ConcurrentDictionary<Type, Func<PocketContainer, object>>();

        private ConcurrentDictionary<Type, object> singletons = new ConcurrentDictionary<Type, object>();

        private Strategy strategyChain = type => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocketContainer"/> class.
        /// </summary>
        public PocketContainer()
        {
            RegisterSingle(c => this);

            AddStrategy(type =>
            {
                // add a default strategy for Func<T> to resolve by convention to return a Func that does a resolve when invoked
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>))
                {
                    return (Func<PocketContainer, object>)
                        GetType()
                            .GetMethod(nameof(MakeResolverFunc), BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(type.GetGenericArguments()[0])
                            .Invoke(this, null);
                }

                return null;
            });

            AfterConstructor();
        }

        /// <summary>
        /// Resolves an instance of the specified type.
        /// </summary>
        public T Resolve<T>()
        {
            var resolved = (T) resolvers.GetOrAdd(typeof(T), _ =>
            {
                var implicitResolver = ImplicitResolver<T>();

                if (Registering?.Invoke(typeof(T), implicitResolver) is Func<PocketContainer, object> replacedResolver)
                {
                    implicitResolver = c => (T) replacedResolver(c);
                }

                return implicitResolver;
            })(this);

            return CallAfterResolve(typeof(T), resolved, out var replaced)
                       ? (T) replaced
                       : resolved;
        }

        /// <summary>
        /// Resolves an instance of the specified type, or default(T) if the type is not registered.
        /// </summary>
        public T ResolveOptional<T>()
        {
            try
            {
                return Resolve<T>();
            }
            catch (Exception ex) when (ex.Data.Contains("ResolveFailed"))
            {
                return default(T);
            }
        }

        /// <summary>
        /// Resolves an instance of the specified type.
        /// </summary>
        public object Resolve(Type type)
        {
            object resolved;

            if (!resolvers.TryGetValue(type, out var func))
            {
                try
                {
                    resolved = resolveMethod.MakeGenericMethod(type).Invoke(this, null);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            else
            {
                resolved = func(this);
            }

            return CallAfterResolve(type, resolved, out var replaced)
                       ? replaced
                       : resolved;
        }

        /// <summary>
        /// Resolves an instance of the specified type, or default(T) if the type is not registered.
        /// </summary>
        public object ResolveOptional(Type type)
        {
            try
            {
                return Resolve(type);
            }
            catch (Exception ex) when (ex.Data.Contains("ResolveFailed"))
            {
                return type.IsValueType
                           ? Activator.CreateInstance(type)
                           : null;
            }
        }

        /// <summary>
        /// Returns an exception to be thrown when resolve fails.
        /// </summary>
        public Func<Type, Exception, Exception> OnFailedResolve =
            (type, exception) =>
                new ArgumentException(
                    $"PocketContainer can't construct a {type} unless you register it first. ☹", exception);

        /// <remarks>When an unregistered type is resolved for the first time, the strategies are checked until one returns a delegate. This delegate will be used in the future to resolve the specified type.</remarks>
        public PocketContainer AddStrategy(
            Strategy strategy,
            bool executeFirst = true)
        {
            var previousStrategy = strategyChain;
            strategyChain = !executeFirst
                                ? (type => previousStrategy(type) ?? strategy(type))
                                : (Strategy) (type => strategy(type) ?? previousStrategy(type));
            return this;
        }

        partial void AfterConstructor();

        /// <summary>
        /// Invoked after each resolve operation. Handlers can return a different object in order to change the result of the resolve operation.
        /// </summary>
        public event Func<Type, object, object> AfterResolve;

        /// <summary>
        /// Invoked when registering a type. Handlers can return a different delegate in order to replace the specified delegate or apply middleware.
        /// </summary>
        public event Func<Type, Delegate, Delegate> Registering;

        /// <summary>
        /// Registers a delegate to retrieve instances of the specified type.
        /// </summary>
        public PocketContainer Register<T>(Func<PocketContainer, T> factory)
        {
            var replaced = (Func<PocketContainer, object>) Registering?.Invoke(typeof(T), factory);
            if (replaced != null)
            {
                factory = c => (T) replaced(c);
            }

            resolvers[typeof(T)] = c => factory(c);
            resolvers[typeof(Lazy<T>)] = c => new Lazy<T>(c.Resolve<T>);
            return this;
        }

        /// <summary>
        /// Registers a delegate to retrieve instances of the specified type.
        /// </summary>
        public PocketContainer Register(Type type, Func<PocketContainer, object> factory)
        {
            registerMethod
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { ConvertFunc(factory, type) });
            return this;
        }

        /// <summary>
        /// Registers a delegate to retrieve an instance of the specified type when it is first resolved. The instance created will be reused for the lifetime of the container.
        /// </summary>
        public PocketContainer RegisterSingle<T>(Func<PocketContainer, T> factory)
        {
            Register(c => (T) singletons.GetOrAdd(typeof(T), t => factory(c)));
            singletons.TryRemove(typeof(T), out object _);
            return this;
        }

        /// <summary>
        /// Registers a delegate to retrieve an instance of the specified type when it is first resolved. The instance created will be reused for the lifetime of the container.
        /// </summary>
        public PocketContainer RegisterSingle(Type type, Func<PocketContainer, object> factory)
        {
            registerSingleMethod
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { ConvertFunc(factory, type) });
            return this;
        }

        /// <summary>
        /// Registers  a delegate to retrieve an instance of the specified type only if it has not previously been registered or successfully resolved.
        /// </summary>
        public PocketContainer TryRegister(
            Type type,
            Func<PocketContainer, object> factory)
        {
            if (!resolvers.ContainsKey(type))
            {
                Register(type, factory);
            }

            return this;
        }

        /// <summary>
        /// Registers  a delegate to retrieve an instance of the specified type only if it has not previously been registered or successfully resolved.
        /// </summary>
        public PocketContainer TryRegister<T>(Func<PocketContainer, T> factory)
        {
            if (!resolvers.ContainsKey(typeof(T)))
            {
                Register(factory);
            }

            return this;
        }

        /// <summary>
        /// Registers  a delegate to retrieve an instance of the specified type only if it has not previously been registered or successfully resolved. The instance created will be reused for the lifetime of the container.
        /// </summary>
        public PocketContainer TryRegisterSingle(
            Type type,
            Func<PocketContainer, object> factory)
        {
            if (!resolvers.ContainsKey(type))
            {
                RegisterSingle(type, factory);
            }

            return this;
        }

        /// <summary>
        /// Registers  a delegate to retrieve an instance of the specified type only if it has not previously been registered or successfully resolved. The instance created will be reused for the lifetime of the container.
        /// </summary>
        public PocketContainer TryRegisterSingle<T>(Func<PocketContainer, T> factory)
        {
            if (!resolvers.ContainsKey(typeof(T)))
            {
                RegisterSingle(factory);
            }

            return this;
        }

        private bool CallAfterResolve(Type type, object resolved, out object replaced)
        {
            if (AfterResolve?.Invoke(type, resolved) is object o)
            {
                singletons.TryUpdate(type, o, resolved);
                replaced = o;
                return true;
            }

            replaced = null;
            return false;
        }

        private Delegate ConvertFunc(Func<PocketContainer, object> func, Type resultType)
        {
            var containerParam = Expression.Parameter(typeof(PocketContainer), "c");

            ConstantExpression constantExpression = null;
            if (func.Target != null)
            {
                constantExpression = Expression.Constant(func.Target);
            }

            var call = Expression.Call(constantExpression, func.GetMethodInfo(), containerParam);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(PocketContainer), resultType);
            var body = Expression.Convert(call, resultType);
            var expression = Expression.Lambda(delegateType,
                                               body,
                                               containerParam);
            return expression.Compile();
        }

        internal static class Factory<T>
        {
            public static readonly Func<PocketContainer, T> Default = Build.UsingLongestConstructor<T>();
        }

        private Func<PocketContainer, object> ImplicitResolver<T>()
        {
            var customFactory = strategyChain(typeof(T));
            if (customFactory != null)
            {
                return customFactory;
            }

            Func<PocketContainer, T> defaultFactory;
            try
            {
                defaultFactory = Factory<T>.Default;
            }
            catch (TypeInitializationException ex)
            {
                if (OnFailedResolve(typeof(T), ex) is Exception ex2)
                {
                    ex2.Data["ResolveFailed"] = true;
                    throw ex2;
                }

                defaultFactory = c => default(T);
            }

            return c => defaultFactory(c);
        }

        internal static class Build
        {
            public static Func<PocketContainer, T> UsingLongestConstructor<T>()
            {
              

                if (typeof(Delegate).IsAssignableFrom(typeof(T)))
                {
                    throw new TypeInitializationException(typeof(T).FullName, null);
                }

                var ctors = typeof(T).GetConstructors();

                var longestCtorParamCount = ctors.Max(c => c.GetParameters().Length);

                var chosenCtor = ctors.Single(c => c.GetParameters().Length == longestCtorParamCount);

                var container = Expression.Parameter(typeof(PocketContainer), "container");

                var factoryExpr = Expression.Lambda<Func<PocketContainer, T>>(
                    Expression.New(
                        chosenCtor,
                        chosenCtor.GetParameters().Select(ResolveParameter)),
                    container);

                return factoryExpr.Compile();

                Expression ResolveParameter(ParameterInfo p) =>
                    p.HasDefaultValue
                        ? (IsNullable(p)
                               ? CreateNullable(p)
                               : IsStructWithNoDefaultValue(p)
                                   ? CreateDefaultStruct(p)
                                   : IsPrimitiveWithDefaultValue(p)
                                       ? UseSpecifiedDefaultValue(p)
                                       : CallResolveOptional(container, p.ParameterType))
                        : CallResolve(container, p.ParameterType);

                bool IsStructWithNoDefaultValue(ParameterInfo p) =>
                    p.ParameterType.IsValueType && Equals(p.DefaultValue, null);

                bool IsNullable(ParameterInfo p) =>
                    p.ParameterType.IsGenericType &&
                    p.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);

                bool IsPrimitiveWithDefaultValue(ParameterInfo p) =>
                    p.HasDefaultValue && (p.ParameterType.IsPrimitive || p.ParameterType == typeof(string));

                Expression CreateDefaultStruct(ParameterInfo p) =>
                    Expression.Constant(Activator.CreateInstance(p.ParameterType), p.ParameterType);

                ConstantExpression UseSpecifiedDefaultValue(ParameterInfo p) =>
                    Expression.Constant(p.DefaultValue, p.ParameterType);

                Expression CreateNullable(ParameterInfo p)
                {
                    var genericArgument = p.ParameterType.GetGenericArguments()[0];

                    var defaultValueForType = Activator.CreateInstance(genericArgument);

                    var argument = Expression.Constant(
                        p.DefaultValue ?? defaultValueForType);

                    var constructor = typeof(Nullable<>)
                                      .MakeGenericType(genericArgument)
                                      .GetConstructor(new[] { genericArgument });

                    return Expression.New(constructor, argument);
                }
            }
        }

        public IEnumerator<KeyValuePair<Type, Func<PocketContainer, object>>> GetEnumerator() => resolvers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static Func<PocketContainer, Func<T>> MakeResolverFunc<T>()
        {
            var container = Expression.Parameter(typeof(PocketContainer), "container");

            var resolve = Expression.Lambda<Func<PocketContainer, Func<T>>>(
                Expression.Lambda<Func<T>>(
                    CallResolve(container, typeof(T))),
                container);

            return resolve.Compile();
        }

        private static Expression CallResolve(ParameterExpression container, Type type) =>
            Expression.Call(container, resolveMethod.MakeGenericMethod(type));

        private static Expression CallResolveOptional(ParameterExpression container, Type type) =>
            Expression.Call(container, resolveOptionalMethod.MakeGenericMethod(type));
    }
}
