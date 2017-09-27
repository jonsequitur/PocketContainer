using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Pocket.Container.Tests
{
    public class PocketContainerBeforeRegisterTests
    {
        [Fact]
        public void BeforeRegister_is_invoked_when_registering_using_Register_T()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.Register(c => "hello");

            container.Resolve<string>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, string>>();
        }

        [Fact]
        public void BeforeRegister_is_invoked_when_registering_using_Register()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.Register(typeof(string), c => "hello");

            container.Resolve<string>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, string>>();
        }

        [Fact]
        public void BeforeRegister_is_invoked_when_registering_using_RegisterSingle_T()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.RegisterSingle(c => "hello");

            container.Resolve<string>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, string>>();
        }

        [Fact]
        public void BeforeRegister_is_invoked_when_registering_using_RegisterSingle()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.RegisterSingle(typeof(string), c => "hello");

            container.Resolve<string>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, string>>();
        }

        [Fact]
        public void BeforeRegister_is_invoked_when_implicit_registration_occurs()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.Resolve<HasDefaultCtor>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, HasDefaultCtor>>();
        }

        [Fact]
        public void BeforeRegister_is_invoked_when_lazy_registration_occurs()
        {
            var receivedDelegates = new List<Delegate>();

            var container = new PocketContainer();

            container.BeforeRegister += receivedDelegates.Add;

            container.AddStrategy(type =>
            {
                if (type == typeof(string))
                {
                    return c => "hello";
                }

                return null;
            });

            container.Resolve<string>();

            receivedDelegates
                .Should()
                .HaveCount(1);
            receivedDelegates
                .Single()
                .Should()
                .BeOfType<Func<PocketContainer, object>>();
        }
    }
}
