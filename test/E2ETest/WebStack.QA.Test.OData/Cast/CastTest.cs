﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Cast
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class CastTest
    {
        private readonly string _namespaceOfEdmSchema = null;
        private static string[] dataSourceTypes = new string[] { "IM", "EF" };// In Memory and Entity Framework

        public CastTest()
        {
            _namespaceOfEdmSchema = typeof(Product).Namespace;
        }

        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(ProductsController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            IEdmModel edmModel = CastEdmModel.GetEdmModel();
            foreach (string dataSourceType in dataSourceTypes)
            {
                configuration.MapODataServiceRoute(dataSourceType, dataSourceType, edmModel);
            }
            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, int> Combinations
        {
            get
            {
                var combinations = new TheoryDataSet<string, string, int>();
                foreach (string dataSourceType in dataSourceTypes)
                {
                    // To Edm.String
                    combinations.Add(dataSourceType, "?$filter=cast('Name1',Edm.String) eq Name", 1);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(Name,Edm.String),'Name')", 3);
                    combinations.Add(dataSourceType, "?$filter=cast(WebStack.QA.Test.OData.Cast.Domain'Civil',Edm.String) eq '2'", 3);
                    combinations.Add(dataSourceType, "?$filter=cast(Domain,Edm.String) eq '3'", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(ID,Edm.String) gt '1'", 2);
                    // TODO bug 1889: Cast function reports error if it is used against a collection of primitive value.
                    // Delete $it after the bug if fixed.
                    combinations.Add(dataSourceType, "(1)/DimensionInCentimeter?$filter=cast($it,Edm.String) gt '1'", 2);
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.String) gt '1.1'", 2);
                    combinations.Add(dataSourceType, "?$filter=contains(cast(ManufacturingDate,Edm.String),'2011')", 1);
                    // TODO bug 1982: The result of casting a value of DateTimeOffset to String is not always the literal representation used in payloads
                    // combinations.Add(dataSourceType, "?$filter=contains(cast(2011-01-01T00:00:00%2B08:00,Edm.String),'2011-01-01')", 3);

                    // To Edm.Int32
                    combinations.Add(dataSourceType, "?$filter=cast(Weight,Edm.Int32) eq 1", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(cast(Name,Edm.Int32),Edm.Int32) eq null", 3);

                    // To DateTimeOffset
                    combinations.Add(dataSourceType, "?$filter=cast(ManufacturingDate,Edm.DateTimeOffset) eq 2011-01-01T00:00:00%2B08:00", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(null,Edm.DateTimeOffset) eq null", 3);

                    // To Enum
                    combinations.Add(dataSourceType, "?$filter=cast('Both',WebStack.QA.Test.OData.Cast.Domain) eq Domain", 1);
                    combinations.Add(dataSourceType, "?$filter=cast('1',WebStack.QA.Test.OData.Cast.Domain) eq Domain", 1);
                    combinations.Add(dataSourceType, "?$filter=cast(null,WebStack.QA.Test.OData.Cast.Domain) eq Domain", 0);
                }

                return combinations;
            }
        }

        [Theory]
        [PropertyData("Combinations")]
        public async Task Query(string dataSourceMode, string dollarFormat, int expectedEntityCount)
        {
            // Arrange
            var requestUri = string.Format("{0}/{1}/Products{2}", this.BaseAddress, dataSourceMode, dollarFormat);

            // Act
            HttpResponseMessage response = await Client.GetAsync(requestUri);
            JObject responseString = await response.Content.ReadAsAsync<JObject>();

            // Assert
            Assert.True(HttpStatusCode.OK == response.StatusCode,
                string.Format("Response status code, expected: {0}, actual: {1}, request url: {2}",
                HttpStatusCode.OK, response.StatusCode, requestUri));

            JArray value = responseString["value"] as JArray;
            Assert.True(expectedEntityCount == value.Count,
                string.Format("The entity count in response, expected: {0}, actual: {1}, request url: {2}",
                expectedEntityCount, value.Count, requestUri));
        }
    }
}
