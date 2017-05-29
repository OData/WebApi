using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.ComplexTypeInheritance
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ComplexTypeInheritanceSerializeTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(MetadataController), typeof(InheritanceCustomersController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.Clear();

            configuration.MapODataServiceRoute(routeName: "odata", routePrefix: "odata", model: GetEdmModel());

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task CanQueryInheritanceComplexInComplexProperty()
        {
            string requestUri = string.Format("{0}/odata/InheritanceCustomers?$format=application/json;odata.metadata=full", BaseAddress);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            string contentOfString = await response.Content.ReadAsStringAsync();

            Assert.True(HttpStatusCode.OK == response.StatusCode,
                String.Format("\nExpected status code: {0},\n actual: {1},\n request uri: {2},\n message: {3}",
                HttpStatusCode.OK,
                response.StatusCode,
                requestUri,
                contentOfString));

            JObject contentOfJObject = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(2, contentOfJObject.Count);
            Assert.Equal(5, contentOfJObject["value"].Count());

            Assert.Equal(new[]
            {
                "#WebStack.QA.Test.OData.ComplexTypeInheritance.InheritanceAddress",
                "#WebStack.QA.Test.OData.ComplexTypeInheritance.InheritanceAddress",
                "#WebStack.QA.Test.OData.ComplexTypeInheritance.InheritanceUsAddress",
                "#WebStack.QA.Test.OData.ComplexTypeInheritance.InheritanceCnAddress",
                "#WebStack.QA.Test.OData.ComplexTypeInheritance.InheritanceCnAddress"
            },
            contentOfJObject["value"].Select(e => e["Location"]["Address"]["@odata.type"]).Select(c => (string)c));
        }

        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<InheritanceCustomer>("InheritanceCustomers");
            builder.ComplexType<InheritanceLocation>();
            return builder.GetEdmModel();
        }
    }

    public class InheritanceCustomersController : ODataController
    {
        private readonly IList<InheritanceCustomer> _customers;
        public InheritanceCustomersController()
        {
            InheritanceAddress address = new InheritanceAddress
            {
                City = "Tokyo",
                Street = "Tokyo Rd"
            };

            InheritanceAddress usAddress = new InheritanceUsAddress
            {
                City = "Redmond",
                Street = "One Microsoft Way",
                ZipCode = 98052
            };

            InheritanceAddress cnAddress = new InheritanceCnAddress
            {
                City = "Shanghai",
                Street = "ZiXing Rd",
                PostCode = "200241"
            };

            _customers = Enumerable.Range(1, 5).Select(e =>
                new InheritanceCustomer
                {
                    Id = e,
                    Location = new InheritanceLocation
                    {
                        Name = "Location #" + e,
                        Address = e < 3 ? address : e < 4 ? usAddress : cnAddress
                    }
                }).ToList();
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_customers);
        }
    }

    public class InheritanceCustomer
    {
        public int Id { get; set; }

        public InheritanceLocation Location { get; set; }
    }

    public class InheritanceLocation
    {
        public string Name { get; set; }

        public InheritanceAddress Address { get; set; }
    }

    public class InheritanceAddress
    {
        public string City { get; set; }

        public string Street { get; set; }
    }

    public class InheritanceUsAddress : InheritanceAddress
    {
        public int ZipCode { get; set; }
    }

    public class InheritanceCnAddress : InheritanceAddress
    {
        public string PostCode { get; set; }
    }
}
