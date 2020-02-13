// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class ETagsUntypedTests : WebHostTestBase<ETagsUntypedTests>
    {
        public ETagsUntypedTests(WebHostTestFixture<ETagsUntypedTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.AddETagMessageHandler(new ETagMessageHandler());
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
        public async Task ModelBuilderTest()
        {
            string expectMetadata =
                "<EntitySet Name=\"ETagUntypedCustomers\" EntityType=\"NS.Customer\">\r\n" +
                "          <Annotation Term=\"Org.OData.Core.V1.OptimisticConcurrency\">\r\n" +
                "            <Collection>\r\n" +
                "              <PropertyPath>Name</PropertyPath>\r\n" +
                "            </Collection>\r\n" +
                "          </Annotation>\r\n" +
                "        </EntitySet>";

            // Remove indentation
            expectMetadata = Regex.Replace(expectMetadata, @"\r\n\s*<", @"<");

            string requestUri = string.Format("{0}/odata/$metadata", this.BaseAddress);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectMetadata, content);

            var stream = await response.Content.ReadAsStreamAsync();
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

            IEdmVocabularyAnnotation valueAnnotation = annotation as IEdmVocabularyAnnotation;
            Assert.NotNull(valueAnnotation);
            Assert.NotNull(valueAnnotation.Value);

            IEdmCollectionExpression collection = valueAnnotation.Value as IEdmCollectionExpression;
            Assert.NotNull(collection);
            Assert.Equal(new[] { "Name" }, collection.Elements.Select(e => ((IEdmPathExpression) e).PathSegments.Single()));
        }

        [Fact]
        public async Task PatchUpdatedEntryWithIfMatchShouldReturnPreconditionFailed()
        {
            string requestUri = this.BaseAddress + "/odata/ETagUntypedCustomers(1)?$format=json";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await this.Client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            var etagInHeader = response.Headers.ETag.ToString();
            JObject result = await response.Content.ReadAsObject<JObject>();
            var etagInPayload = (string)result["@odata.etag"];

            Assert.True(etagInPayload == etagInHeader);
            Assert.Equal("W/\"J1NhbSc=\"", etagInPayload);
        }
    }

    public class ETagUntypedCustomersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer = new EdmEntityObject(entityType);
            customer.TrySetPropertyValue("ID", key);
            customer.TrySetPropertyValue("Name", "Sam");
            return Ok(customer);
        }
    }
}