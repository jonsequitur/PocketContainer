// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using FluentAssertions;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerDependencyResolverStrategyTests
    {
        [Test]
        public void Top_level_types_resolved_from_the_dependency_resolver()
        {
            var resolver = new FakeDependencyResolver(t => "hola!");
            var container = new PocketContainer().IncludeDependencyResolver(resolver);

            container.Resolve<string>().Should().Be("hola!");
        }

        [Test]
        public void Second_level_dependencies_are_resolved_from_the_dependency_resolver()
        {
            var innerContainer = new PocketContainer()
                .Register(c => "bonjour!");
            var resolver = new FakeDependencyResolver(innerContainer.Resolve);
            var container = new PocketContainer().IncludeDependencyResolver(resolver);

            var obj = container.Resolve<HasOneParamCtor<string>>();

            obj.Should().NotBeNull();
            obj.Value1.Should().Be("bonjour!");
        }
    }

    public class FakeDependencyResolver : IDependencyResolver
    {
        private readonly Func<Type, object> getService;

        public FakeDependencyResolver(Func<Type, object> getService)
        {
            this.getService = getService;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return getService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}