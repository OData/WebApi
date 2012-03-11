using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Services
{
    public class DependencyResolverTests
    {
        // TODO: Add tests for SetService and GetCachedService

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<DependencyResolver>(TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void ConstructorThrowsOnNullConfig()
        {
            Assert.ThrowsArgumentNull(() => new DependencyResolver(null), "configuration");
            Assert.ThrowsArgumentNull(() => new DependencyResolver(null, null), "configuration");
        }

        [Fact]
        public void ConstructorWithUserNullDependencyResolver()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config, null);

            // Act
            object service = resolver.GetService(typeof(IHttpActionSelector));

            // Assert
            Assert.Null(service);
        }

        [Fact]
        public void ConstructorWithUserDependencyResolver()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<IDependencyResolver> userResolverMock = new Mock<IDependencyResolver>();
            IHttpActionSelector actionSelector = new Mock<IHttpActionSelector>().Object;
            userResolverMock.Setup(ur => ur.GetService(typeof(IHttpActionSelector))).Returns(actionSelector).Verifiable();
            DependencyResolver resolver = new DependencyResolver(config, userResolverMock.Object);

            // Act
            object service = resolver.GetService(typeof(IHttpActionSelector));

            // Assert
            userResolverMock.Verify();
            Assert.Same(actionSelector, service);
        }

        [Fact]
        public void GetServiceThrowsOnNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config, null);

            // Act
            Assert.ThrowsArgumentNull(() => resolver.GetService(null), "serviceType");
        }

        [Fact]
        public void GetServiceDoesntEagerlyCreate()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);

            // Act
            object result = resolver.GetService(typeof(SomeClass));

            // Assert
            // Service resolver should not have created an instance or arbitrary class.
            Assert.Null(result);
        }

        [Fact]
        public void GetServicesThrowsOnNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config, null);

            // Act
            Assert.ThrowsArgumentNull(() => resolver.GetServices(null), "serviceType");
        }

        [Fact]
        public void GetServicesDoesntEagerlyCreate()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);

            // Act
            IEnumerable<object> result = resolver.GetServices(typeof(SomeClass));

            // Assert
            // Service resolver should not have created an instance or arbitrary class.
            Assert.Empty(result);
        }

        // Arbitrary test class that we can use with the service resolver
        internal class SomeClass
        {
        }

        [Fact]
        public void SetResolverIDependencyResolverThrowsOnNull()
        {
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);
            Assert.ThrowsArgumentNull(() => resolver.SetResolver((IDependencyResolver)null), "resolver");
        }

        [Fact]
        public void SetResolverIDependencyResolver()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);

            Mock<IDependencyResolver> userResolverMock = new Mock<IDependencyResolver>();
            IHttpActionSelector actionSelector = new Mock<IHttpActionSelector>().Object;
            userResolverMock.Setup(ur => ur.GetService(typeof(IHttpActionSelector))).Returns(actionSelector).Verifiable();
            userResolverMock.Setup(ur => ur.GetServices(typeof(IHttpActionSelector))).Returns(new List<object> { actionSelector }).Verifiable();

            resolver.SetResolver(userResolverMock.Object);

            // Act
            object service = resolver.GetService(typeof(IHttpActionSelector));
            IEnumerable<object> services = resolver.GetServices(typeof(IHttpActionSelector));

            // Assert
            userResolverMock.Verify();
            Assert.Same(actionSelector, service);
            Assert.Same(actionSelector, services.ElementAt(0));
        }

        [Fact]
        public void SetResolverCommonServiceLocatorThrowsOnNull()
        {
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);
            Assert.ThrowsArgumentNull(() => resolver.SetResolver((object)null), "commonServiceLocator");
        }

        [Fact]
        public void SetResolverCommonServiceLocator()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);

            Mock<CommonServiceLocatorSlim> userResolverMock = new Mock<CommonServiceLocatorSlim>();
            IHttpActionSelector actionSelector = new Mock<IHttpActionSelector>().Object;
            userResolverMock.Setup(ur => ur.GetInstance(typeof(IHttpActionSelector))).Returns(actionSelector).Verifiable();
            userResolverMock.Setup(ur => ur.GetAllInstances(typeof(IHttpActionSelector))).Returns(new List<object> { actionSelector }).Verifiable();

            resolver.SetResolver(userResolverMock.Object);

            // Act
            object service = resolver.GetService(typeof(IHttpActionSelector));
            IEnumerable<object> services = resolver.GetServices(typeof(IHttpActionSelector));

            // Assert
            userResolverMock.Verify();
            Assert.Same(actionSelector, service);
            Assert.Same(actionSelector, services.ElementAt(0));
        }

        public interface CommonServiceLocatorSlim
        {
            object GetInstance(Type serviceType);

            IEnumerable<object> GetAllInstances(Type serviceType);
        }

        [Fact]
        public void SetResolverFuncThrowsOnNull()
        {
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);
            Assert.ThrowsArgumentNull(() => resolver.SetResolver(null, _ => null), "getService");
            Assert.ThrowsArgumentNull(() => resolver.SetResolver(_ => null, null), "getServices");
        }

        [Fact]
        public void SetResolverFunc()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            DependencyResolver resolver = new DependencyResolver(config);

            Mock<CommonServiceLocatorSlim> userResolverMock = new Mock<CommonServiceLocatorSlim>();
            IHttpActionSelector actionSelector = new Mock<IHttpActionSelector>().Object;

            resolver.SetResolver(_ => actionSelector, _ => new List<object> { actionSelector });

            // Act
            object service = resolver.GetService(typeof(IHttpActionSelector));
            IEnumerable<object> services = resolver.GetServices(typeof(IHttpActionSelector));

            // Assert
            userResolverMock.Verify();
            Assert.Same(actionSelector, service);
            Assert.Same(actionSelector, services.ElementAt(0));
        }
    }
}
