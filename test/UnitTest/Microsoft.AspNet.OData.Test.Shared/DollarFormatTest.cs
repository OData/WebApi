//-----------------------------------------------------------------------------
// <copyright file="DollarFormatTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class DollarFormatTest
    {
        [Theory]
        [InlineData("FormatCustomers", "application/json;odata.metadata=full")]
        [InlineData("This", "application/json;odata.metadata=full")]
        public async Task DollarFormat_Applies_IfPresent(string path, string mediaTypeFormat)
        {
            // Arrange
            MediaTypeHeaderValue expected = MediaTypeHeaderValue.Parse(mediaTypeFormat);
            string url = string.Format("http://localhost/{0}?$format={1}", path, mediaTypeFormat);
            IEdmModel model = GetEdmModel();
            var configuration = RoutingConfigurationFactory.CreateWithTypes(
                new[] { typeof(FormatCustomersController), typeof(ThisController) });
            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);
            configuration.MapODataServiceRoute("odata", routePrefix: null, model: model);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(expected, response.Content.Headers.ContentType);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("FormatCustomers");
            builder.Singleton<Customer>("This"); // Singleton
            return builder.GetEdmModel();
        }

        public class Customer
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class FormatCustomersController : ODataController
        {
            public Customer[] Get()
            {
                return new Customer[] { };
            }
        }

        public class ThisController : ODataController
        {
            public Customer Get()
            {
                return new Customer();
            }
        }
    }
}
#endif
