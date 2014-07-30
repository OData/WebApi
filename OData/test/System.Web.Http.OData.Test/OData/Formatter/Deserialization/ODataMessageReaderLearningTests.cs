// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataMessageReaderLearningTests
    {
        [Fact]
        public void TestCreateODataCollectionReader_InJsonLight_WithoutTypeReference_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.CreateODataCollectionReader());
            }
        }

        [Fact]
        public void TestCreateODataCollectionReader_InJsonLight_WithTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmFunctionImport producingFunctionImport = model.EntityContainers().Single().FunctionImports().First();
            IEdmTypeReference expectedItemTypeReference = new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.CreateODataCollectionReader(expectedItemTypeReference));
            }
        }

        [Fact]
        public void TestCreateODataEntryReader_InJsonLight_WithoutEntitySetOrType_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.CreateODataEntryReader());
            }
        }

        [Fact]
        public void TestCreateODataEntryReader_InJsonLight_WithEntityTypeButWithoutSet_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntityType entityType = model.EntityContainers().Single().EntitySets().First().ElementType;

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.CreateODataEntryReader(null, entityType));
            }
        }

        [Fact]
        public void TestCreateODataEntryReader_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.CreateODataEntryReader(entitySet, null));
            }
        }

        [Fact]
        public void TestCreateODataFeedReader_InJsonLight_WithoutEntitySetOrType_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.CreateODataFeedReader());
            }
        }

        [Fact]
        public void TestCreateODataFeedReader_InJsonLight_WithEntityTypeButWithoutSet_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntityType entityType = model.EntityContainers().Single().EntitySets().First().ElementType;

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.CreateODataFeedReader(entityType));
            }
        }

        [Fact]
        public void TestCreateODataFeedReader_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.CreateODataFeedReader(entitySet, null));
            }
        }

        [Fact]
        public void TestReadEntityReferenceLink_InJsonLight_WithoutNavigationProperty_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest("{\"url\":\"aa:b\"}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => reader.ReadEntityReferenceLink());
            }
        }

        [Fact]
        public void TestReadEntityReferenceLink_InJsonLight_WithNavigationProperty_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest("{\"url\":\"aa:b\"}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmNavigationProperty navigationProperty =
                model.EntityContainers().Single().EntitySets().First().NavigationTargets.First().NavigationProperty;

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.ReadEntityReferenceLink(navigationProperty));
            }
        }

        [Fact]
        public void TestReadProperty_InJsonLight_WithoutStructuralPropertyOrTypeReference_DoesNotThrows()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.ReadProperty());
            }
        }

        [Fact]
        public void TestReadProperty_InJsonLight_WithStructuralProperty_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmStructuralProperty property = model.EntityContainers().Single().EntitySets().First().ElementType.StructuralProperties().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.ReadProperty(property));
            }
        }

        [Fact]
        public void TestReadProperty_InJsonLight_WithTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateJsonLightRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmTypeReference expectedPropertyTypeReference = new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => reader.ReadProperty(expectedPropertyTypeReference));
            }
        }

        private static IODataRequestMessage CreateJsonLightRequest()
        {
            HttpContentHeaders headers;

            using (HttpContent content = new StreamContent(Stream.Null))
            {
                headers = content.Headers;
            }

            headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata");

            return new ODataMessageWrapper(Stream.Null, headers);
        }

        private static IODataRequestMessage CreateJsonLightRequest(string body)
        {
            HttpContent content = new StringContent(body);
            HttpContentHeaders headers = content.Headers;
            headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata");

            return new ODataMessageWrapper(content.ReadAsStreamAsync().Result, headers);
        }

        private static IEdmModel CreateModel()
        {
            var model = new EdmModel();

            var orderType = new EdmEntityType("Default", "Order");
            orderType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            model.AddElement(orderType);

            var customerType = new EdmEntityType("Default", "Customer");
            customerType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32);
            model.AddElement(customerType);

            // Add navigations
            orderType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo() { Name = "Customer",
                Target = customerType, TargetMultiplicity = EdmMultiplicity.One });

            var container = new EdmEntityContainer("Default", "Container");
            var orderSet = container.AddEntitySet("Orders", orderType);
            var customerSet = container.AddEntitySet("Customers", customerType);

            container.AddFunctionImport("GetIDs", new EdmCollectionTypeReference(new EdmCollectionType(
                new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32),
                false)), false));

            orderSet.AddNavigationTarget(orderType.NavigationProperties().Single(np => np.Name == "Customer"),
                customerSet);

            model.AddElement(container);
            return model;
        }

        private static ODataMessageReaderSettings CreateSettings()
        {
            return new ODataMessageReaderSettings();
        }
    }
}
