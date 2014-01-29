// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.TestCommon;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ODataValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider_ThrowsArgumentNull_ActionContext()
        {
            ODataValueProviderFactory factory = new ODataValueProviderFactory();
            Assert.ThrowsArgumentNull(() => factory.GetValueProvider(actionContext: null), "actionContext");
        }

        [Fact]
        public void GetValueProvider_ReturnsValueProvider_BackedByRoutingStore()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().RoutingConventionsStore["ID"] = 42;
            HttpActionContext actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };
            ODataValueProviderFactory factory = new ODataValueProviderFactory();

            // Act
            var valueProvider = factory.GetValueProvider(actionContext);

            // Assert
            Assert.NotNull(valueProvider);
            Assert.Equal(42, valueProvider.GetValue("ID").RawValue);
        }

        [Fact]
        public void CanModelBindNonStringData()
        {
            // Arrange
            HttpServer server = new HttpServer();
            MockAssembly assembly = new MockAssembly(typeof(TestController));
            server.Configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<TestClass>("Test");
            server.Configuration.Routes.MapODataServiceRoute("odata", "", builder.GetEdmModel());
            HttpClient client = new HttpClient(server);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Test");
            request.ODataProperties().RoutingConventionsStore["test"] = new TestClass { Id = 42 };
            var response = client.SendAsync(request).Result;

            // Assert
            TestClass result = response.Content.ReadAsAsync<TestClass>().Result;
            Assert.Equal(42, result.Id);
        }

        public class TestClass
        {
            public int Id { get; set; }
        }

        public class TestController : ODataController
        {
            public TestClass Get([FromUri]TestClass test)
            {
                return test;
            }
        }
    }
}
