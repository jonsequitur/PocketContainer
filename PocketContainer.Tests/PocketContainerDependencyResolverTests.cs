using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Dependencies;
using NUnit.Framework;

namespace Pocket.Tests
{
    [TestFixture]
    public class PocketContainerDependencyResolverTests
    {
        [SetUp]
        public void SetUp()
        {
            DisposeCounter.DisposeCount = 0;
        }

        [Test]
        public void When_framework_dependency_constructor_cannot_be_satisfied_then_exception_is_swallowed()
        {
            var container = new PocketContainer();
            container.Register<IContentNegotiator>(c => c.Resolve<UnsastisfiableDependency>());
            var sut = new PocketContainerDependencyResolver(container);

            Action resolveFrameworkDependency =
                () => sut.GetService(typeof (IContentNegotiator));
            Action resolveFrameworkDependencies =
                () => sut.GetServices(typeof (IContentNegotiator)).ToArray();

            resolveFrameworkDependency.ShouldNotThrow("because we're resolving a framework service");
            resolveFrameworkDependencies.ShouldNotThrow("because we're resolving a framework service");
        }

        [Test]
        public void When_nonframework_dependency_constructor_cannot_be_satisfied_then_exception_is_thrown()
        {
            var container = new PocketContainer();
            container.Register<IMyDependency>(c => c.Resolve<UnsastisfiableDependency>());
            var sut = new PocketContainerDependencyResolver(container);

            Action resolveFrameworkDependency =
                () => sut.GetService(typeof (IMyDependency));

            resolveFrameworkDependency.ShouldThrow<ArgumentException>("because GetService is resolving our own interface")
                .And
                .Message
                .Should()
                .Contain("+IAmUnregistered");
        }

        [Test]
        public void When_framework_dependency_constructor_throws_then_exception_is_thrown()
        {
            var container = new PocketContainer();
            container.Register<IContentNegotiator>(c => new DependencyThatThrowOnConstruction());
            var sut = new PocketContainerDependencyResolver(container);

            Action resolveFrameworkDependency =
                () => sut.GetService(typeof (IContentNegotiator));

            resolveFrameworkDependency.ShouldThrow<HttpParseException>(
                "because framework service implementation constructor threw an exception");
        }

        [Test]
        public void When_nonframework_dependency_constructor_throws_then_exception_is_thrown()
        {
            var container = new PocketContainer();
            container.Register<IMyDependency>(c => new DependencyThatThrowOnConstruction());
            var sut = new PocketContainerDependencyResolver(container);

            Action resolveFrameworkDependency =
                () => sut.GetService(typeof (IMyDependency));

            resolveFrameworkDependency.ShouldThrow<HttpParseException>("because GetService is resolving our own interface");
        }

        public class DependencyThatThrowOnConstruction : IContentNegotiator, IMyDependency
        {
            public DependencyThatThrowOnConstruction()
            {
                throw new HttpParseException("boo");
            }

            public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request,
                IEnumerable<MediaTypeFormatter> formatters)
            {
                throw new NotImplementedException();
            }
        }

        public interface IMyDependency
        {
        }

        public class UnsastisfiableDependency : IContentNegotiator, IMyDependency
        {
            public UnsastisfiableDependency(IAmUnregistered unregisteredInterface)
            {
            }

            public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request,
                IEnumerable<MediaTypeFormatter> formatters)
            {
                throw new NotImplementedException();
            }
        }

        public interface IAmUnregistered
        {
        }
    }

    public class DisposeCounter : IDisposable
    {
        public void Dispose()
        {
            DisposeCount++;
        }

        public static int DisposeCount { get; set; }
    }
}