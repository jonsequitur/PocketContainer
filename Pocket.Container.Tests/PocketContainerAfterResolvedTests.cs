using System;
using FluentAssertions;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerAfterResolvedTests
    {
        [Fact]
        public void AfterResolve_is_invoked_after_a_transient_registration_is_resolved_using_Resolve_T()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .Register(c => "hello");

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve<string>();

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }

        [Fact]
        public void AfterResolve_is_invoked_after_a_transient_registration_is_resolved_using_Resolve()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .Register(c => "hello");

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve(typeof(string));

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }

        [Fact]
        public void AfterResolve_is_invoked_after_a_singleton_registration_is_resolved_using_Resolve_T()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .RegisterSingle(c => "hello");

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve<string>();

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }

        [Fact]
        public void AfterResolve_is_invoked_after_a_singleton_registration_is_resolved_using_Resolve()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .RegisterSingle(c => "hello");

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve(typeof(string));

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }

        [Fact]
        public void AfterResolve_is_invoked_after_a_lazy_registration_is_resolved_using_Resolve_T()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .AddStrategy(type =>
                {
                    if (type == typeof(string))
                    {
                        return c => "hello";
                    }

                    return null;
                });

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve<string>();

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }

        [Fact]
        public void AfterResolve_is_invoked_after_a_lazy_registration_is_resolved_using_Resolve()
        {
            Type typeArg = null;
            object instanceArg = null;

            var container = new PocketContainer()
                .AddStrategy(type =>
                {
                    if (type == typeof(string))
                    {
                        return c => "hello";
                    }

                    return null;
                });

            container.AfterResolve += (type, instance) =>
            {
                typeArg = type;
                instanceArg = instance;
                return instance;
            };

            container.Resolve(typeof(string));

            typeArg.Should().Be(typeof(string));
            instanceArg.Should().Be("hello");
        }
    }
}
