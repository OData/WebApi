// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
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
    }
}
