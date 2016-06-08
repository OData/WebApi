// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.DateAndTimeOfDay
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class DateWithEfTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] {typeof (EfPeopleController)};
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            IEdmModel model = DateAndTimeOfDayEdmModel.BuildEfPersonEdmModel();

            // TODO: modify it after implement the DI in Web API.
            // model.SetPayloadValueConverter(new MyConverter());

            configuration.Routes.Clear();
            configuration.MapODataServiceRoute("odata", "odata", model);

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("$filter=Birthday eq null", "2,4")]
        [InlineData("$filter=Birthday ne null", "1,3,5")]
        [InlineData("$filter=Birthday eq 2015-10-01", "1")]
        [InlineData("$filter=Birthday eq 2015-10-03", "3")]
        [InlineData("$filter=Birthday ne 2015-10-03", "1,2,4,5")]
        public async Task CanFilterByDatePropertyForDateTimePropertyOnEf(string filter, string expect)
        {
            string requestUri = string.Format("{0}/odata/EfPeople?{1}", BaseAddress, filter);

            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsAsync<JObject>();

            Assert.Equal(expect, String.Join(",", content["value"].Select(e => e["Id"].ToString())));
        }
    }

    public class MyConverter : ODataPayloadValueConverter
    {
        public override object ConvertToPayloadValue(object value, IEdmTypeReference edmTypeReference)
        {
            if (edmTypeReference != null && edmTypeReference.IsDate())
            {
                if (value is DateTimeOffset)
                {
                    DateTimeOffset dto = (DateTimeOffset)value;
                    return new Date(dto.Year, dto.Month, dto.Day);
                }
            }

            return base.ConvertToPayloadValue(value, edmTypeReference);
        }
    }
}
