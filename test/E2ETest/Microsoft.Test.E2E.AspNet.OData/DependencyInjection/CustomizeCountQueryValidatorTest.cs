// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DependencyInjection
{
    public class CustomizeCountQueryValidatorTest : WebHostTestBase<CustomizeCountQueryValidatorTest>
    {
        private const string CustomerBaseUrl = "{0}/customcountvalidator/Customers";
        private const string OrderBaseUrl = "{0}/customcountvalidator/Orders";

        public CustomizeCountQueryValidatorTest(WebHostTestFixture<CustomizeCountQueryValidatorTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(CustomersController), typeof(OrdersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("customcountvalidator", "customcountvalidator", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => EdmModel.GetEdmModel(configuration))
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting("customcountvalidator", configuration))
                       .AddService<CountQueryValidator, MyCountQueryValidator>(ServiceLifetime.Singleton));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$count=true", (int)HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$count=true", (int)HttpStatusCode.OK)]
        public async Task CutomizeCountValidator(string entitySetUrl, int statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            Assert.Equal(statusCode, (int)response.StatusCode);
            if (statusCode == (int)HttpStatusCode.BadRequest)
            {
                Assert.Contains("cannot be used for $count", result);
            }
        }
    }

    public class MyCountQueryValidator : CountQueryValidator
    {
        public MyCountQueryValidator(DefaultQuerySettings defaultQuerySettings)
            :base(defaultQuerySettings)
        {
        }

        public override void Validate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
        {
            if (countQueryOption.Context.ElementClrType == typeof(Customer))
            {
                base.Validate(countQueryOption, validationSettings);
            }
        }
    }
}