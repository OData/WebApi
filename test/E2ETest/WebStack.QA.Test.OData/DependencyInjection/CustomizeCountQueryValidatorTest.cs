// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.DependencyInjection
{
    public class CustomizeCountQueryValidatorTest : ODataTestBase
    {
        private const string CustomerBaseUrl = "{0}/dependencyinjection/Customers";
        private const string OrderBaseUrl = "{0}/dependencyinjection/Orders";

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(typeof(CustomersController), typeof(OrdersController)));
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("dependencyinjection", "dependencyinjection", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => EdmModel.GetEdmModel())
                       .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting("dependencyinjection", configuration))
                       .AddService<CountQueryValidator, MyCountQueryValidator>(ServiceLifetime.Singleton));
        }

        [Theory]
        [InlineData(CustomerBaseUrl + "?$count=true", HttpStatusCode.BadRequest)]
        [InlineData(OrderBaseUrl + "?$count=true", HttpStatusCode.OK)]
        public void CutomizeCountValidator(string entitySetUrl, HttpStatusCode statusCode)
        {
            string queryUrl =
                string.Format(
                    entitySetUrl,
                    BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(statusCode, response.StatusCode);
            if (statusCode == HttpStatusCode.BadRequest)
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