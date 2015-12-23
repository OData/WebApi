using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.QueryComposition.Controllers;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class ApplyTests : ODataTestBase
    {

        public static TheoryDataSet<string, IQueryable> TestApplies
        {
            get
            {
                return new TheoryDataSet<string, IQueryable>
                {
                    {
                        "Products?$apply=groupby((Name))", DataSource.EfProducts.GroupBy(p => new { p.Name }).Select(g => new { Name = g.Key.Name })
                    },
                    {
                        "Products?$apply=groupby((Name), aggregate(Rating with average as AvgRating))", DataSource.EfProducts.GroupBy(p => new { p.Name }).Select(g => new { Name = g.Key.Name, AvgRating = g.Average(p=> p.Rating) })
                    },
                    {
                        "OrderLines?$apply=aggregate(Cost with sum as Cost)", DataSource.EfOrderLines.GroupBy(p => new { }).Select(g => new { Cost = g.Sum(ol => ol.Cost) })
                    },

                    {
                        "OrderLines?$apply=filter(Product/Name eq 'Kinect')/aggregate(Cost with sum as TotalCost)", DataSource.EfOrderLines.Where(ol => ol.Product.Name == "Kinect").GroupBy(p => new { }).Select(g => new { TotalCost = g.Sum(ol => ol.Cost) })
                    },
                    {
                        "OrderLines?$apply=groupby((Product/Name),aggregate(Cost with sum as TotalCost))/filter(TotalCost ge 250)", DataSource.EfOrderLines.GroupBy(p => new { ProductName = p.Product.Name }).Select(g => new { Product = new { Name=g.Key.ProductName },  TotalCost = g.Sum(ol => ol.Cost) }).Where(g =>g.TotalCost >= 250M)
                    },
                };
            }
        }

        [Theory]
        [PropertyData("TestApplies")]
        public void CanApply(string clauses, IQueryable expectedQuery)
        {
            var response = this.Client.GetAsync(this.BaseAddress + $"/api/ApplyTests/Get{clauses}").Result;
            var resultString = response.Content.ReadAsStringAsync().Result;
            var result = JArray.Parse(resultString);
            var expectedList = (expectedQuery as IQueryable<dynamic>).ToList();


            Assert.Equal(expectedList.Count(), result.Count());
            for (int i = 0; i < expectedList.Count(); i++)
            {
                var actual = result.ElementAt(i);
                var expected = JObject.FromObject(expectedList.ElementAt(i));
                Assert.Equal<dynamic>(expected, actual);
            }
        }
    }
}
