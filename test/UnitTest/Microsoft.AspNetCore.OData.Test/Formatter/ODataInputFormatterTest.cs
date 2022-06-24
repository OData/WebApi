//-----------------------------------------------------------------------------
// <copyright file="ODataInputFormatterTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------
#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Logging;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataInputFormatterTest
    {

        [Fact]
        public async Task  ExceptionNotThrownWhenILoggerServiceIsDefinedAndFlagIsNotSet()
        {
            // Arrange
            const string requestbody = "{\"ID\":2,\"Name\":2}";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(CustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
            }, (configureService) =>
            {
                configureService.AddSingleton<ILogger, TestLogger>();

            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Customers"))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.Content = new StringContent(requestbody);
                
                // Act
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            }
        }

        [Fact]
        public async Task ExceptionThrownWhenILoggerServiceIsDefinedAndFlagIsSet()
        {
            // Arrange
            const string requestbody = "{\"ID\":2,\"Name\":2}";

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            var controllers = new[] { typeof(CustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", null, model);
                config.SetCompatibilityOptions(CompatibilityOptions.ThrowExceptionAfterLoggingModelStateError);
            });

            using (HttpClient client = TestServerFactory.CreateClient(server))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Customers"))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.Content = new StringContent(requestbody);

                // Act
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Assert
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                }
            }
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class CustomersController : ODataController
        {
            private List<Customer> customers = new List<Customer>()
        {
            new Customer()
            {
                ID = 1,
                Name = "Jane",
            }
        };

            [EnableQuery]
            public IActionResult Post([FromBody] Customer customer)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                customers.Add(customer);
                return Ok();

            }
        }

        private class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }
        }
    }
}
