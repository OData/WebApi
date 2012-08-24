// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;
using SignalR;
using SignalR.Hubs;

namespace System.Web.Http
{
    public class HubControllerBaseTest
    {
        [Fact]
        public void Clients_ThrowsInvalidOperationForNullHubContext()
        {
            Assert.Throws<InvalidOperationException>(
                () => new HubContextController(null).Clients,
                "No hub context could be found for the HubController of type 'HubContextController'.");
        }

        [Fact]
        public void Clients_ReturnsHubContextsClients()
        {
            Mock<IHubContext> mockContext = new Mock<IHubContext>();
            object clients = new object();
            mockContext.Setup(mock => mock.Clients).Returns(clients);
            var controller = new HubContextController(mockContext.Object);

            Assert.Same(clients, controller.Clients as object);
        }

        [Fact]
        public void Groups_ThrowsInvalidOperationForNullHubContext()
        {
            Assert.Throws<InvalidOperationException>(
                () => new HubContextController(null).Groups,
                "No hub context could be found for the HubController of type 'HubContextController'.");
        }

        [Fact]
        public void Groups_ReturnsHubContextsClients()
        {
            Mock<IHubContext> mockContext = new Mock<IHubContext>();
            IGroupManager groups = new Mock<IGroupManager>().Object;
            mockContext.Setup(mock => mock.Groups).Returns(groups);
            var controller = new HubContextController(mockContext.Object);

            Assert.Same(groups, controller.Groups);
        }

        [Fact]
        public void ConnectionManager_ReturnsGlobalConnectionManager_IfConfigurationIsNull()
        {
            HubContextController controller = new HubContextController();

            Assert.Same(GlobalHost.ConnectionManager, controller.GetConnectionManager());
        }

        [Fact]
        public void ConnectionManager_ReturnsGlobalConnectionManager_IfCannotResolveIConnectionManager()
        {
            HubContextController controller = new HubContextController();
            controller.Configuration = new HttpConfiguration();
            Mock<System.Web.Http.Dependencies.IDependencyResolver> mockDependencyResolver = new Mock<Dependencies.IDependencyResolver>();
            mockDependencyResolver.Setup(mock => mock.GetService(typeof(IConnectionManager))).Returns(null);
            controller.Configuration.DependencyResolver = mockDependencyResolver.Object;

            Assert.Same(GlobalHost.ConnectionManager, controller.GetConnectionManager());
        }

        [Fact]
        public void ConnectionManager_ReturnsConnectionManagerFromDependencyResolver_IfFound()
        {
            HubContextController controller = new HubContextController();
            controller.Configuration = new HttpConfiguration();
            IConnectionManager connectionManager = new Mock<IConnectionManager>().Object;
            Mock<System.Web.Http.Dependencies.IDependencyResolver> mockDependencyResolver = new Mock<Dependencies.IDependencyResolver>();
            mockDependencyResolver.Setup(mock => mock.GetService(typeof(IConnectionManager))).Returns(connectionManager);
            controller.Configuration.DependencyResolver = mockDependencyResolver.Object;

            Assert.Same(connectionManager, controller.GetConnectionManager());
        }

        public class HubContextController : HubControllerBase
        {
            IHubContext _hubContext;

            public HubContextController(IHubContext hubContext = null)
            {
                _hubContext = hubContext;
            }

            protected override IHubContext HubContext
            {
                get
                {
                    return _hubContext;
                }
            }

            public IConnectionManager GetConnectionManager()
            {
                return ConnectionManager;
            }
        }
    }
}