//-----------------------------------------------------------------------------
// <copyright file="ODataValueProviderFactoryTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;
#else
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Xunit;
#endif

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider_ThrowsArgumentNull_ActionContext()
        {
            ODataValueProviderFactory factory = new ODataValueProviderFactory();
            ExceptionAssert.ThrowsArgumentNull(() => factory.GetValueProvider(actionContext: null), "actionContext");
        }

        [Fact]
        public void GetValueProvider_ReturnsValueProvider_BackedByRoutingStore()
        {
            // Arrange
            var request = RequestFactory.Create();
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
        public async Task CanModelBindNonStringData()
        {
            // Arrange
            HttpServer server = new HttpServer();
            MockAssembly assembly = new MockAssembly(typeof(TestController));
            server.Configuration.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver(assembly));

            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<TestClass>("Test");
            server.Configuration.MapODataServiceRoute("odata", "", builder.GetEdmModel());
            HttpClient client = new HttpClient(server);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Test");
            request.ODataProperties().RoutingConventionsStore["test"] = new TestClass { Id = 42 };
            var response = await client.SendAsync(request);

            // Assert
            TestClass result = await response.Content.ReadAsObject<TestClass>();
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
#endif
