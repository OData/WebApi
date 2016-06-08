// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    public class ETagsUntypedTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MessageHandlers.Add(new ETagMessageHandler());
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            IEdmStructuralProperty customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            EdmEntitySet customers = container.AddEntitySet("ETagUntypedCustomers", customer);

            model.SetOptimisticConcurrencyAnnotation(customers, new[] { customerName });

            return model;
        }

        [Fact]
        public void ModelBuilderTest()
        {
            const string expectMetadata =
                "        <EntitySet Name=\"ETagUntypedCustomers\" EntityType=\"NS.Customer\">\r\n" +
                "          <Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">\r\n" +
                "            <Collection>\r\n" +
                "              <PropertyPath>Name</PropertyPath>\r\n" +
                "            </Collection>\r\n" +
                "          </Annotation>\r\n" +
                "        </EntitySet>";

            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = this.Client.GetAsync(requestUri).Result;

            var content = response.Content.ReadAsStringAsync().Result;
            Assert.Contains(expectMetadata, content);

            var stream = response.Content.ReadAsStreamAsync().Result;
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();
            Assert.NotNull(edmModel);

            var etagCustomers = edmModel.FindDeclaredEntitySet("ETagUntypedCustomers");
            Assert.NotNull(etagCustomers);

            var annotations = edmModel.FindDeclaredVocabularyAnnotations(etagCustomers);
            IEdmVocabularyAnnotation annotation = Assert.Single(annotations);
            Assert.NotNull(annotation);

            Assert.Same(CoreVocabularyModel.ConcurrencyTerm, annotation.Term);
            Assert.Same(etagCustomers, annotation.Target);

            IEdmValueAnnotation valueAnnotation = annotation as IEdmValueAnnotation;
            Assert.NotNull(valueAnnotation);
            Assert.NotNull(valueAnnotation.Value);

            IEdmCollectionExpression collection = valueAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(collection);
            Assert.Equal(new[] { "Name" }, collection.Elements.Select(e => ((IEdmPathExpression) e).Path.Single()));
        }

        [Fact]
        public void PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagUntypedCustomers(1)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = this.Client.SendAsync(request).Result;

            Assert.True(response.IsSuccessStatusCode);
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = response.Content.ReadAsAsync<JObject>().Result;
            var etagInPayload = (string)result["@odata.etag"];

            Assert.True(etagInPayload == etagInHeader);
            Assert.Equal(etagInPayload, "W/\"J1NhbSc=\"");
        }
    }

    public class ETagUntypedCustomersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            IEdmModel model = Request.ODataProperties().Model;
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer = new EdmEntityObject(entityType);
            customer.TrySetPropertyValue("ID", key);
            customer.TrySetPropertyValue("Name", "Sam");
            return Ok(customer);
        }
    }
}