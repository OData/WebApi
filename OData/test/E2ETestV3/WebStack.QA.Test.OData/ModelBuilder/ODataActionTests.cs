using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;
using ActionTestsClient = ODataClient.ProductsContextReference.Service;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class ODataActionTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(), "odata");
        }

        // ExtendSupportDate of a single Product
        [Theory]
        [InlineData("ODataActionTests_Products1", "ExtendSupportDate1")]
        [InlineData("ODataActionTests_Products2", "ExtendSupportDate2")]
        public void SingleEntityBoundActionTest(string entitySetName, string actionName)
        {
            ActionTestsClient.Container cntr = new ActionTestsClient.Container(new Uri(BaseAddress));

            DateTime dt = Convert.ToDateTime("2025-01-01T00:00:00");
            var product = cntr.Execute<ODataActionTests_Product>(new Uri(string.Format("{0}/odata/{1}(1)/{2}", BaseAddress, entitySetName, actionName)),
                                        "POST", true,
                                        new BodyOperationParameter("newDate", dt)).Single();

            Assert.NotNull(product);
            Assert.Equal(1, product.ID);
            Assert.True(product.SupportedUntil.Equals(dt));
        }

        // ExtendSupportDates of a collection of Products
        [Theory]
        [InlineData("ODataActionTests_Products1", "ExtendSupportDates1")]
        [InlineData("ODataActionTests_Products2", "ExtendSupportDates2")]
        public void CollectionOfEntitiesBoundActionTest(string entitySetName, string actionName)
        {
            ActionTestsClient.Container cntr = new ActionTestsClient.Container(new Uri(BaseAddress));

            DateTime dt = Convert.ToDateTime("2025-01-01T00:00:00");
            var products = cntr.Execute<ODataActionTests_Product>(new Uri(string.Format("{0}/odata/{1}/{2}", BaseAddress, entitySetName, actionName)),
                                                        "POST", true,
                                                        new BodyOperationParameter("productIds", new int[] { 1, 2, 3, 4 }),
                                                        new BodyOperationParameter("newDate", dt)).ToList();

            Assert.NotNull(products);
            Assert.Equal(4, products.Count());

            foreach (ODataActionTests_Product prod in products)
            {
                Assert.True(prod.SupportedUntil.Equals(dt));
            }
        }

        // UpdateRating of a single RatedProduct
        [Theory]
        [InlineData("ODataActionTests_Products1", "UpdateRating1")]
        [InlineData("ODataActionTests_Products2", "UpdateRating2")]
        public void SingleInheritedEntityBoundActionTest(string entitySetName, string actionName)
        {
            ActionTestsClient.Container cntr = new ActionTestsClient.Container(new Uri(BaseAddress));

            int newRating = 8;
            var product = cntr.Execute<ODataActionTests_Product>(new Uri(string.Format("{0}/odata/{1}(4)/WebStack.QA.Test.OData.ModelBuilder.ODataActionTests_RatedProduct/{2}", BaseAddress, entitySetName, actionName)),
                                                        "POST", true,
                                                        new BodyOperationParameter("newRating", newRating)).Single();

            Assert.NotNull(product);
            Assert.Equal(4, product.ID);
            Assert.Equal(newRating, ((ODataActionTests_RatedProduct)product).Rating);
        }

        // UpdateRatings of a collection of inherited RatedProducts
        [Theory]
        [InlineData("ODataActionTests_Products1", "UpdateRatings1")]
        [InlineData("ODataActionTests_Products2", "UpdateRatings2")]
        public void CollectionOfInheritedEntitiesBoundActionTest(string entitySetName, string actionName)
        {
            ActionTestsClient.Container cntr = new ActionTestsClient.Container(new Uri(BaseAddress));

            int newRating = 10;
            var products = cntr.Execute<ODataActionTests_Product>(new Uri(string.Format("{0}/odata/{1}/WebStack.QA.Test.OData.ModelBuilder.ODataActionTests_RatedProduct/{2}", BaseAddress, entitySetName, actionName)),
                                                        "POST", true,
                                                       new BodyOperationParameter("productIds", new int[] { 4 }),
                                                       new BodyOperationParameter("newRating", newRating)).ToList();

            Assert.NotNull(products);
            Assert.Equal(1, products.Count());

            foreach (ODataActionTests_Product prod in products)
            {
                Assert.Equal(newRating, ((ODataActionTests_RatedProduct)prod).Rating);
            }
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var products1 = builder.EntitySet<ODataActionTests_Product>("ODataActionTests_Products1");
            var products2 = builder.EntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // ExtendSupportDate of a single Product
            var extendSupportDate1 = products1.EntityType.TransientAction("ExtendSupportDate1");
            extendSupportDate1.Parameter<DateTime>("newDate");
            extendSupportDate1.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // ExtendSupportDates on multiple Products
            var extendSupportDates1 = products1.EntityType.Collection.TransientAction("ExtendSupportDates1");
            extendSupportDates1.CollectionParameter<int>("productIds");
            extendSupportDates1.Parameter<DateTime>("newDate");
            extendSupportDates1.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // UpdateRating of a single RatedProduct
            var updateRating1 = builder.Entity<ODataActionTests_RatedProduct>().TransientAction("UpdateRating1");
            updateRating1.Parameter<int>("newRating");
            updateRating1.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // UpdateRatings of a single RatedProduct
            var updateRatings1 = builder.Entity<ODataActionTests_RatedProduct>().Collection.TransientAction("UpdateRatings1");
            updateRatings1.CollectionParameter<int>("productIds");
            updateRatings1.Parameter<int>("newRating");
            updateRatings1.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // ExtendSupportDate of a single Product
            var extendSupportDate2 = products1.EntityType.TransientAction("ExtendSupportDate2");
            extendSupportDate2.Parameter<DateTime>("newDate");
            extendSupportDate2.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // ExtendSupportDates on multiple Products
            var extendSupportDates2 = products1.EntityType.Collection.TransientAction("ExtendSupportDates2");
            extendSupportDates2.CollectionParameter<int>("productIds");
            extendSupportDates2.Parameter<DateTime>("newDate");
            extendSupportDates2.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // UpdateRating of a single RatedProduct
            var updateRating2 = builder.Entity<ODataActionTests_RatedProduct>().TransientAction("UpdateRating2");
            updateRating2.Parameter<int>("newRating");
            updateRating2.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // UpdateRatings of a single RatedProduct
            var updateRatings2 = builder.Entity<ODataActionTests_RatedProduct>().Collection.TransientAction("UpdateRatings2");
            updateRatings2.CollectionParameter<int>("productIds");
            updateRatings2.Parameter<int>("newRating");
            updateRatings2.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            return builder.GetEdmModel();
        }

        public static string GetModelStateErrorInformation(ModelStateDictionary modelState)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid request received.");

            if (modelState != null)
            {
                foreach (var key in modelState.Keys)
                {
                    if (modelState[key].Errors.Count > 0)
                    {
                        sb.AppendLine(key + ":" + modelState[key].Value.RawValue);
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class ODataActionTests_Products1Controller : ODataController
    {
        private List<ODataActionTests_Product> products = null;

        public ODataActionTests_Products1Controller()
        {
            products = new List<ODataActionTests_Product>();
            products.Add(new ODataActionTests_Product { ID = 1, Name = "IE7", ReleaseDate = new DateTime(2007, 1, 1), SupportedUntil = new DateTime(2008, 1, 1) });
            products.Add(new ODataActionTests_Product { ID = 2, Name = "IE8", ReleaseDate = new DateTime(2008, 1, 1), SupportedUntil = new DateTime(2009, 1, 1) });
            products.Add(new ODataActionTests_Product { ID = 3, Name = "IE9", ReleaseDate = new DateTime(2009, 1, 1), SupportedUntil = new DateTime(2010, 1, 1) });
            products.Add(new ODataActionTests_RatedProduct { ID = 4, Name = "IE10", Rating = 5, ReleaseDate = new DateTime(2010, 1, 1), SupportedUntil = new DateTime(2011, 1, 1) });
        }

        public ODataActionTests_Product ExtendSupportDate1([FromODataUri]int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            ODataActionTests_Product product = products.Where(prod => prod.ID == key).SingleOrDefault();

            if (product == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            product.SupportedUntil = Convert.ToDateTime(parameters["newDate"].ToString());

            return product;
        }

        public IEnumerable<ODataActionTests_Product> ExtendSupportDates1(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            foreach (ODataActionTests_Product prd in products)
            {
                prd.SupportedUntil = Convert.ToDateTime(parameters["newDate"].ToString());
            }

            return products;
        }

        public ODataActionTests_Product UpdateRating1OnODataActionTests_RatedProduct([FromODataUri]int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            ODataActionTests_Product ratedProduct = products.Where(prod => prod.ID == key).SingleOrDefault();

            if (ratedProduct == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());

            return ratedProduct;
        }

        public IEnumerable<ODataActionTests_Product> UpdateRatings1OnCollectionOfODataActionTests_RatedProduct(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            IEnumerable<ODataActionTests_Product> ratedProducts = products.OfType<ODataActionTests_RatedProduct>();

            foreach (ODataActionTests_RatedProduct ratedProduct in ratedProducts)
            {
                ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());
            }

            return ratedProducts;
        }
    }

    public class ODataActionTests_Products2Controller : EntitySetController<ODataActionTests_Product, int>
    {
        private List<ODataActionTests_Product> products = null;

        public ODataActionTests_Products2Controller()
        {
            products = new List<ODataActionTests_Product>();
            products.Add(new ODataActionTests_Product { ID = 1, Name = "IE7", ReleaseDate = new DateTime(2007, 1, 1), SupportedUntil = new DateTime(2008, 1, 1) });
            products.Add(new ODataActionTests_Product { ID = 2, Name = "IE8", ReleaseDate = new DateTime(2008, 1, 1), SupportedUntil = new DateTime(2009, 1, 1) });
            products.Add(new ODataActionTests_Product { ID = 3, Name = "IE9", ReleaseDate = new DateTime(2009, 1, 1), SupportedUntil = new DateTime(2010, 1, 1) });
            products.Add(new ODataActionTests_RatedProduct { ID = 4, Name = "IE10", Rating = 5, ReleaseDate = new DateTime(2010, 1, 1), SupportedUntil = new DateTime(2011, 1, 1) });
        }

        public ODataActionTests_Product ExtendSupportDate2([FromODataUri]int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            ODataActionTests_Product product = products.Where(prod => prod.ID == key).SingleOrDefault();

            if (product == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            product.SupportedUntil = Convert.ToDateTime(parameters["newDate"].ToString());

            return product;
        }

        public IEnumerable<ODataActionTests_Product> ExtendSupportDates2(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            foreach (ODataActionTests_Product prd in products)
            {
                prd.SupportedUntil = Convert.ToDateTime(parameters["newDate"].ToString());
            }

            return products;
        }

        public ODataActionTests_Product UpdateRating2OnODataActionTests_RatedProduct([FromODataUri]int key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            ODataActionTests_Product ratedProduct = products.Where(prod => prod.ID == key).SingleOrDefault();

            if (ratedProduct == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());

            return ratedProduct;
        }

        public IEnumerable<ODataActionTests_Product> UpdateRatings2OnCollectionOfODataActionTests_RatedProduct(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }

            IEnumerable<ODataActionTests_Product> ratedProducts = products.OfType<ODataActionTests_RatedProduct>();

            foreach (ODataActionTests_RatedProduct ratedProduct in ratedProducts)
            {
                ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());
            }

            return ratedProducts;
        }
    }

    public class ODataActionTests_Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public DateTime? SupportedUntil { get; set; }
    }

    public class ODataActionTests_RatedProduct : ODataActionTests_Product
    {
        public int Rating { get; set; }
    }
}