// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
{
    public class EntitySetControllerHelpersTest
    {
        [Fact]
        public void PostResponse_Throws_IfODataPathMissing()
        {
            ApiController controller = new Mock<ApiController>().Object;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Customers");
            controller.Configuration = new HttpConfiguration();
            controller.Request = request;

            Assert.Throws<InvalidOperationException>(
                () => EntitySetControllerHelpers.PostResponse<Customer, int>(controller, new Customer(), 5),
                "A Location header could not be generated because the request does not have an associated OData path.");
        }

        [Fact]
        public void PostResponse_Throws_IfODataPathDoesNotStartWithEntitySet()
        {
            ApiController controller = new Mock<ApiController>().Object;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Customers");
            controller.Configuration = new HttpConfiguration();
            request.ODataProperties().Path = new ODataPath(new MetadataPathSegment());
            controller.Request = request;

            Assert.Throws<InvalidOperationException>(
                () => EntitySetControllerHelpers.PostResponse<Customer, int>(controller, new Customer(), 5),
                "A Location header could not be generated because the request's OData path does not start with an entity set path segment.");
        }

        [Fact]
        public void CreateQueryOptions_SetsContextProperties_WithModelAndPath()
        {
            // Arrange
            ApiController controller = new Mock<ApiController>().Object;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Customers");
            controller.Configuration = new HttpConfiguration();
            ODataModelBuilder odataModel = new ODataModelBuilder();
            string setName = typeof(Customer).Name;
            odataModel.EntityType<Customer>().HasKey(c => c.Id);
            odataModel.EntitySet<Customer>(setName);
            IEdmModel model = odataModel.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(setName);
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment(entitySet));
            controller.Request = request;

            // Act
            ODataQueryOptions<Customer> queryOptions =
                EntitySetControllerHelpers.CreateQueryOptions<Customer>(controller);

            // Assert
            Assert.Same(model, queryOptions.Context.Model);
            Assert.Same(entitySet, queryOptions.Context.NavigationSource);
            Assert.Same(typeof(Customer), queryOptions.Context.ElementClrType);
        }
    }
}
