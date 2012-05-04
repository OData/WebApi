using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.Query
{
    public class QueryableTest
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("text/xml")]
        public void QueryableAttributeWorks(string mediaType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Queryable?$filter=ID ge 5&$orderby=ZipCode&$top=3&$skip=1");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            ScenarioHelper.RunTest(
                "Queryable",
                String.Empty,
                request,
                response =>
                {
                    Assert.NotNull(response);
                    Assert.True(response.IsSuccessStatusCode);

                    QueryableController.Customer[] customers = response.Content.ReadAsAsync<QueryableController.Customer[]>().Result;
                    Assert.NotNull(customers);

                    QueryableController controller = new QueryableController();
                    var expectedCustomers = controller.Get()
                                            .Where(customer => customer.ID >= 5)
                                            .OrderBy(customer => customer.ZipCode)
                                            .Skip<QueryableController.Customer>(1)
                                            .Take<QueryableController.Customer>(3)
                                            .ToArray();
                    Assert.Equal(expectedCustomers, customers);
                });
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/xml")]
        public void QueryableAttributeWithCustomQueryBuilder(string mediaType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Queryable?$filter=ID ge 5&$orderby=ZipCode&$top=3&$skip=1&$select=ID,Name");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            ScenarioHelper.RunTest(
                "Queryable",
                String.Empty,
                request,
                response =>
                {
                    Assert.NotNull(response);
                    Assert.True(response.IsSuccessStatusCode);
                    QueryableController.Customer[] customers = response.Content.ReadAsAsync<QueryableController.Customer[]>().Result;
                    Assert.NotNull(customers);

                    QueryableController controller = new QueryableController();
                    var expectedCustomers = controller.Get()
                                            .Where(customer => customer.ID >= 5)
                                            .OrderBy(customer => customer.ZipCode)
                                            .Skip<QueryableController.Customer>(1)
                                            .Take<QueryableController.Customer>(3)
                                            .Select(customer => new QueryableController.Customer { ID = customer.ID, Name = customer.Name })
                                            .ToArray();
                    Assert.Equal(expectedCustomers, customers);
                },
                configuration =>
                {
                    configuration.Services.Replace(typeof(IStructuredQueryBuilder), new StructuredQueryBuilderPlus());
                });
        }

    }
}
