using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Extensions;
using Microsoft.OData.Core.UriParser.Metadata;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.UriParserExtension
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class UnqualifiedCallTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(MetadataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.SetUriResolver(new UnqualifiedODataUriResolver());

            configuration.Routes.Clear();

            configuration.MapODataServiceRoute(routeName: "odata",
                routePrefix: "odata", model: UriParserExtenstionEdmModel.GetEdmModel());

            configuration.EnsureInitialized();
        }

        public static TheoryDataSet<string, string, string> UnqualifiedCallCases
        {
            get
            {
                return new TheoryDataSet<string, string, string>()
                {
                    { "Get", "Customers(1)/Default.CalculateSalary(month=2)", "Customers(1)/CalculateSalary(month=2)" },
                    { "Post", "Customers(1)/Default.UpdateAddress()", "Customers(1)/UpdateAddress()" },
                };
            }
        }

        [Theory]
        [PropertyData("UnqualifiedCallCases")]
        public async Task EnableUnqualifiedCallTest(string method, string caseSensitive, string caseInsensitive)
        {
            // Case sensitive
            var caseSensitiveUri = string.Format("{0}/odata/{1}", this.BaseAddress, caseSensitive);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), caseSensitiveUri);
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string caseSensitiveResponse = await response.Content.ReadAsStringAsync();

            // Case Insensitive
            var caseInsensitiveUri = string.Format("{0}/odata/{1}", this.BaseAddress, caseInsensitive);
            request = new HttpRequestMessage(new HttpMethod(method), caseInsensitiveUri);
            response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string caseInsensitiveResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(caseSensitiveResponse, caseInsensitiveResponse);
        }
    }
}
