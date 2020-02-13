// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;
using Container = ModelBuilder.ActionTest.Default.Container;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class ODataActionTests : WebHostTestBase<ODataActionTests>
    {
        public ODataActionTests(WebHostTestFixture<ODataActionTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration), "odata");
        }

#if !NETCORE // TODO #939: Enable this test for AspNetCore
        // ExtendSupportDate of a single Product
        [Theory]
        [InlineData("ODataActionTests_Products1", "ExtendSupportDate1")]
        [InlineData("ODataActionTests_Products2", "ExtendSupportDate2")]
        public async Task SingleEntityBoundActionTest(string entitySetName, string actionName)
        {
            Container cntr = new Container(new Uri(BaseAddress));

            DateTimeOffset dt = new DateTimeOffset(Convert.ToDateTime("2025-01-01T00:00:00"));

            var product = (await cntr.ExecuteAsync<ODataActionTests_Product>(
                new Uri(string.Format("{0}/odata/{1}(1)/Default.{2}", BaseAddress, entitySetName, actionName)),
                "POST",
                true,
                new BodyOperationParameter("newDate", dt))).Single();

            Assert.NotNull(product);
            Assert.Equal(1, product.ID);
            Assert.True(product.SupportedUntil.Equals(dt));
        }

        // ExtendSupportDates of a collection of Products
        [Theory]
        [InlineData("ODataActionTests_Products1", "ExtendSupportDates1")]
        [InlineData("ODataActionTests_Products2", "ExtendSupportDates2")]
        public async Task CollectionOfEntitiesBoundActionTest(string entitySetName, string actionName)
        {
            Container cntr = new Container(new Uri(BaseAddress));

            DateTimeOffset dt = new DateTimeOffset(Convert.ToDateTime("2025-01-01T00:00:00"));
            var products = (await cntr.ExecuteAsync<ODataActionTests_Product>(
                new Uri(string.Format("{0}/odata/{1}/Default.{2}", BaseAddress, entitySetName, actionName)),
                "POST",
                true,
                new BodyOperationParameter("productIds", new int[] { 1, 2, 3, 4 }),
                new BodyOperationParameter("newDate", dt))).ToList();

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
        public async Task SingleInheritedEntityBoundActionTest(string entitySetName, string actionName)
        {
            Container cntr = new Container(new Uri(BaseAddress));

            int newRating = 8;
            var product = (await cntr.ExecuteAsync<ODataActionTests_Product>(
                new Uri(string.Format("{0}/odata/{1}(4)/Microsoft.Test.E2E.AspNet.OData.ModelBuilder.ODataActionTests_RatedProduct/Default.{2}", BaseAddress, entitySetName, actionName)),
                "POST",
                true,
                new BodyOperationParameter("newRating", newRating))).Single();

            Assert.NotNull(product);
            Assert.Equal(4, product.ID);
            Assert.Equal(newRating, ((ODataActionTests_RatedProduct)product).Rating);
        }

        // UpdateRatings of a collection of inherited RatedProducts
        [Theory]
        [InlineData("ODataActionTests_Products1", "UpdateRatings1")]
        [InlineData("ODataActionTests_Products2", "UpdateRatings2")]
        public async Task CollectionOfInheritedEntitiesBoundActionTest(string entitySetName, string actionName)
        {
            Container cntr = new Container(new Uri(BaseAddress));

            int newRating = 10;
            var products = (await cntr.ExecuteAsync<ODataActionTests_Product>(
                new Uri(string.Format("{0}/odata/{1}/Microsoft.Test.E2E.AspNet.OData.ModelBuilder.ODataActionTests_RatedProduct/Default.{2}", BaseAddress, entitySetName, actionName)),
                "POST",
                true,
                new BodyOperationParameter("productIds", new int[] { 4 }),
                new BodyOperationParameter("newRating", newRating))).ToList();

            Assert.NotNull(products);
            Assert.Single(products);

            foreach (ODataActionTests_Product prod in products)
            {
                Assert.Equal(newRating, ((ODataActionTests_RatedProduct)prod).Rating);
            }
        }
#endif
        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var products1 = builder.EntitySet<ODataActionTests_Product>("ODataActionTests_Products1");
            var products2 = builder.EntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // ExtendSupportDate of a single Product
            var extendSupportDate1 = products1.EntityType.Action("ExtendSupportDate1");
            extendSupportDate1.Parameter<DateTimeOffset>("newDate");
            extendSupportDate1.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // ExtendSupportDates on multiple Products
            var extendSupportDates1 = products1.EntityType.Collection.Action("ExtendSupportDates1");
            extendSupportDates1.CollectionParameter<int>("productIds");
            extendSupportDates1.Parameter<DateTimeOffset>("newDate");
            extendSupportDates1.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // UpdateRating of a single RatedProduct
            var updateRating1 = builder.EntityType<ODataActionTests_RatedProduct>().Action("UpdateRating1");
            updateRating1.Parameter<int>("newRating");
            updateRating1.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // UpdateRatings of a single RatedProduct
            var updateRatings1 = builder.EntityType<ODataActionTests_RatedProduct>().Collection.Action("UpdateRatings1");
            updateRatings1.CollectionParameter<int>("productIds");
            updateRatings1.Parameter<int>("newRating");
            updateRatings1.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products1");

            // ExtendSupportDate of a single Product
            var extendSupportDate2 = products1.EntityType.Action("ExtendSupportDate2");
            extendSupportDate2.Parameter<DateTimeOffset>("newDate");
            extendSupportDate2.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // ExtendSupportDates on multiple Products
            var extendSupportDates2 = products1.EntityType.Collection.Action("ExtendSupportDates2");
            extendSupportDates2.CollectionParameter<int>("productIds");
            extendSupportDates2.Parameter<DateTimeOffset>("newDate");
            extendSupportDates2.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // UpdateRating of a single RatedProduct
            var updateRating2 = builder.EntityType<ODataActionTests_RatedProduct>().Action("UpdateRating2");
            updateRating2.Parameter<int>("newRating");
            updateRating2.ReturnsFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            // UpdateRatings of a single RatedProduct
            var updateRatings2 = builder.EntityType<ODataActionTests_RatedProduct>().Collection.Action("UpdateRatings2");
            updateRatings2.CollectionParameter<int>("productIds");
            updateRatings2.Parameter<int>("newRating");
            updateRatings2.ReturnsCollectionFromEntitySet<ODataActionTests_Product>("ODataActionTests_Products2");

            return builder.GetEdmModel();
        }

#if !NETCORE // TODO #939: Enable this test for AspNetCore
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
#endif
    }

    public class ODataActionTests_Products1Controller : TestODataController
    {
        private List<ODataActionTests_Product> products = null;

        public ODataActionTests_Products1Controller()
        {
            products = new List<ODataActionTests_Product>();
            products.Add(new ODataActionTests_Product { ID = 1, Name = "IE7", ReleaseDate = new DateTimeOffset(new DateTime(2007, 1, 1)), SupportedUntil = new DateTime(2008, 1, 1) });
            products.Add(new ODataActionTests_Product { ID = 2, Name = "IE8", ReleaseDate = new DateTimeOffset(new DateTime(2008, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2009, 1, 1)) });
            products.Add(new ODataActionTests_Product { ID = 3, Name = "IE9", ReleaseDate = new DateTimeOffset(new DateTime(2009, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2010, 1, 1)) });
            products.Add(new ODataActionTests_RatedProduct { ID = 4, Name = "IE10", Rating = 5, ReleaseDate = new DateTimeOffset(new DateTime(2010, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2011, 1, 1)) });
        }

        public ODataActionTests_Product ExtendSupportDate1([FromODataUri]int key, ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif

            ODataActionTests_Product product = products.Where(prod => prod.ID == key).SingleOrDefault();

#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (product == null)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
#endif

            product.SupportedUntil = new DateTimeOffset(Convert.ToDateTime(parameters["newDate"].ToString()));
            return product;
        }

        public IEnumerable<ODataActionTests_Product> ExtendSupportDates1(ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif
            foreach (ODataActionTests_Product prd in products)
            {
                prd.SupportedUntil = new DateTimeOffset(Convert.ToDateTime(parameters["newDate"].ToString()));
            }

            return products;
        }

        public ODataActionTests_Product UpdateRating1OnODataActionTests_RatedProduct([FromODataUri]int key, ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif

            ODataActionTests_Product ratedProduct = products.Where(prod => prod.ID == key).SingleOrDefault();

#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (ratedProduct == null)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
#endif

            ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());

            return ratedProduct;
        }

        public IEnumerable<ODataActionTests_Product> UpdateRatings1OnCollectionOfODataActionTests_RatedProduct(ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif

            IEnumerable<ODataActionTests_Product> ratedProducts = products.OfType<ODataActionTests_RatedProduct>();

            foreach (ODataActionTests_RatedProduct ratedProduct in ratedProducts)
            {
                ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());
            }

            return ratedProducts;
        }
    }

    public class ODataActionTests_Products2Controller : TestODataController
    {
        private List<ODataActionTests_Product> products = null;

        public ODataActionTests_Products2Controller()
        {
            products = new List<ODataActionTests_Product>();
            products.Add(new ODataActionTests_Product { ID = 1, Name = "IE7", ReleaseDate = new DateTimeOffset(new DateTime(2007, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2008, 1, 1)) });
            products.Add(new ODataActionTests_Product { ID = 2, Name = "IE8", ReleaseDate = new DateTimeOffset(new DateTime(2008, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2009, 1, 1)) });
            products.Add(new ODataActionTests_Product { ID = 3, Name = "IE9", ReleaseDate = new DateTimeOffset(new DateTime(2009, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2010, 1, 1)) });
            products.Add(new ODataActionTests_RatedProduct { ID = 4, Name = "IE10", Rating = 5, ReleaseDate = new DateTimeOffset(new DateTime(2010, 1, 1)), SupportedUntil = new DateTimeOffset(new DateTime(2011, 1, 1)) });
        }

        public ODataActionTests_Product ExtendSupportDate2([FromODataUri]int key, ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif

            ODataActionTests_Product product = products.Where(prod => prod.ID == key).SingleOrDefault();

#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (product == null)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
#endif

            product.SupportedUntil = new DateTimeOffset(Convert.ToDateTime(parameters["newDate"].ToString()));

            return product;
        }

        public IEnumerable<ODataActionTests_Product> ExtendSupportDates2(ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif
            foreach (ODataActionTests_Product prd in products)
            {
                prd.SupportedUntil = new DateTimeOffset(Convert.ToDateTime(parameters["newDate"].ToString()));
            }

            return products;
        }

        public ODataActionTests_Product UpdateRating2OnODataActionTests_RatedProduct([FromODataUri]int key, ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif
            ODataActionTests_Product ratedProduct = products.Where(prod => prod.ID == key).SingleOrDefault();

#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (ratedProduct == null)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
#endif
            ((ODataActionTests_RatedProduct)ratedProduct).Rating = Convert.ToInt32(parameters["newRating"].ToString());

            return ratedProduct;
        }

        public IEnumerable<ODataActionTests_Product> UpdateRatings2OnCollectionOfODataActionTests_RatedProduct(ODataActionParameters parameters)
        {
#if !NETCORE // TODO #939: Enable this check for AspNetCore
            if (!ModelState.IsValid)
            {
                throw new System.Web.Http.HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ODataError() { Message = ODataActionTests.GetModelStateErrorInformation(this.ModelState) }));
            }
#endif
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

        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset? SupportedUntil { get; set; }
    }

    public class ODataActionTests_RatedProduct : ODataActionTests_Product
    {
        public int Rating { get; set; }
    }
}