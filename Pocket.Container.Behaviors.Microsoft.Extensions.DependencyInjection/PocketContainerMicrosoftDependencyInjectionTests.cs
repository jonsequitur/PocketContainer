// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pocket.Container.Tests;
using Xunit;

namespace Pocket.Container.Microsoft.Extensions.DependencyInjection.Tests
{
    public class PocketContainerMicrosoftDependencyInjectionTests
    {
        [Fact]
        public void Resolve_implements_IServiceProvider_GetService()
        {
            var container = new PocketContainer()
                                    .Register(_ => "hello")
                                as IServiceProvider;

            var resolved = container.GetService(typeof(string));

            resolved.Should().Be("hello");
        }

        [Fact]
        public void When_a_type_cannot_be_resolved_via_GetService_it_returns_null()
        {
            var container = new PocketContainer() as IServiceProvider;

            var resolved = container.GetService(typeof(string));

            resolved.Should().BeNull();
        }

        [Fact]
        public void When_a_type_cannot_be_resolved_via_GetRequiredService_it_throws()
        {
            var container = new PocketContainer() as ISupportRequiredService;

            Action resolve = () => container.GetRequiredService(typeof(string));

            resolve.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void AddSingleton_behaves_as_expected()
        {
            var services = new ServiceCollection()
                .AddSingleton<IEnumerable<string>>(new List<string>());
            var container = new PocketContainer()
                .AsServiceProvider(services);

            var fromContainer = container.GetService<IEnumerable<string>>();
            var fromServices = services.BuildServiceProvider()
                                       .GetService<IEnumerable<string>>();

            fromContainer.Should().BeSameAs(fromServices);
        }

        [Fact]
        public void AddTransient_behaves_as_expected()
        {
            var services = new ServiceCollection()
                .AddTransient<IAmAnInterface, HasDefaultCtor>();
            var container = new PocketContainer()
                .AsServiceProvider(services);

            var fromContainer = container.GetService<IAmAnInterface>();
            var fromServices = services.BuildServiceProvider()
                                       .GetService<IAmAnInterface>();

            fromContainer.Should().BeOfType<HasDefaultCtor>();
            fromContainer.Should().NotBeSameAs(fromServices);
        }

        [Fact]
        public void AddFactory_behaves_as_expected()
        {
            var services = new ServiceCollection()
                .AddTransient<IAmAnInterface>(s => new HasDefaultCtor());
            var container = new PocketContainer()
                .AsServiceProvider(services);

            var fromContainer = container.GetService<IAmAnInterface>();
            var fromServices = services.BuildServiceProvider()
                                       .GetService<IAmAnInterface>();

            fromContainer.Should().BeOfType<HasDefaultCtor>();
            fromContainer.Should().NotBeSameAs(fromServices);
        }

        [Fact]
        public void IEnumerable_is_resolvable_when_multiple_transients_have_been_registered()
        {
            var services = new ServiceCollection()
                .AddTransient<IAmAnInterface, HasDefaultCtor>()
                .AddTransient<IAmAnInterface, AlsoHasDefaultCtor>();

            var container = new PocketContainer()
                .AsServiceProvider(services);

            var fromServices = services.BuildServiceProvider()
                                       .GetService<IEnumerable<IAmAnInterface>>();

            var fromContainer = container.GetService<IEnumerable<IAmAnInterface>>();

            fromContainer.Count().Should().Be(2);
            fromContainer.Should().Contain(o => o is HasDefaultCtor);
            fromContainer.Should().Contain(o => o is AlsoHasDefaultCtor);
        }

        [Fact]
        public void EXAMPLE_Singleton_instances_resolved_from_the_root_are_disposed_when_the_container_is_disposed()
        {
            var wasDisposed = false;

            var services = new ServiceCollection()
                .AddSingleton(_ => Create.Disposable(() => { wasDisposed = true; }));

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<IDisposable>();

            ((IDisposable) serviceProvider).Dispose();

            wasDisposed.Should().BeTrue();
        }

        [Fact]
        public void EXAMPLE_Transient_instances_resolved_from_the_root_are_disposed_when_the_container_is_disposed()
        {
            var wasDisposed = false;

            var services = new ServiceCollection()
                .AddTransient(_ => Create.Disposable(() => { wasDisposed = true; }));

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<IDisposable>();

            ((IDisposable) serviceProvider).Dispose();

            wasDisposed.Should().BeTrue();
        }

        [Fact]
        public void EXAMPLE_Singleton_instances_resolved_from_a_scope_are_not_disposed_when_the_scope_is_disposed()
        {
            var wasDisposed = false;

            var services = new ServiceCollection()
                .AddSingleton(_ => Create.Disposable(() => { wasDisposed = true; }));

            var scope = services.BuildServiceProvider().CreateScope();

            scope.ServiceProvider.GetService<IDisposable>();

            scope.Dispose();

            wasDisposed.Should().BeFalse();
        }

        [Fact]
        public void EXAMPLE_Transient_instances_resolved_from_a_scope_are_disposed_when_the_scope_is_disposed()
        {
            var wasDisposed = false;

            var services = new ServiceCollection()
                .AddTransient(_ => Create.Disposable(() => { wasDisposed = true; }));

            var scope = services.BuildServiceProvider().CreateScope();

            scope.ServiceProvider.GetService<IDisposable>();

            scope.Dispose();

            wasDisposed.Should().BeTrue();
        }

        [Fact]
        public void hmm_really()
        {
            var services = new ServiceCollection();

            services.AddSingleton("hello");
            services.AddSingleton("hello2");

            var x = services.BuildServiceProvider();

            var oneString = x.GetService<string>();
            var twoStrings = x.GetService<IEnumerable<string>>();

            oneString.Should().Be("hello2");
            twoStrings.Should().BeEquivalentTo("hello", "hello2");
        }
    }

    internal static class Create
    {
        public static IDisposable Disposable(Action dispose) =>
            new AnonymousDisposable(dispose);

        private class AnonymousDisposable : IDisposable
        {
            private readonly Action dispose;
            public AnonymousDisposable(Action dispose) => this.dispose = dispose;

            public void Dispose() => dispose();
        }
    }
}
