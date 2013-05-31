// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataMessageWriterLearningTests
    {
        [Fact]
        public void TestCreateODataCollectionWriter_InJsonLight_WithoutTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataCollectionWriter());
            }
        }

        [Fact]
        public void TestCreateODataCollectionWriter_InJsonLight_WithTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            IEdmTypeReference itemTypeReference = new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataCollectionWriter(itemTypeReference));
            }
        }

        [Fact]
        public void TestCreateODataEntryWriter_InJsonLight_WithoutEntitySetOrType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataEntryWriter());
            }
        }

        [Fact]
        public void TestCreateODataEntryWriter_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataEntryWriter(entitySet));
            }
        }

        [Fact]
        public void TestCreateODataFeedWriter_InJsonLight_WithoutEntitySetOrType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataFeedWriter());
            }
        }

        [Fact]
        public void TestCreateODataFeedWriter_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.CreateODataFeedWriter(entitySet));
            }
        }

        [Fact]
        public void TestWriteEntityReferenceLink_InJsonLight_WithoutEntitySetOrNavigationProperty_Throws()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink
            {
                Url = CreateFakeUri()
            };

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => writer.WriteEntityReferenceLink(link));
            }
        }

        [Fact]
        public void TestWriteEntityReferenceLink_InJsonLight_WithEntitySetButNotNavigationProperty_Throws()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink
            {
                Url = CreateFakeUri()
            };
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.Throws<ODataException>(() => writer.WriteEntityReferenceLink(link, entitySet, null));
            }
        }

        [Fact]
        public void TestWriteEntityReferenceLink_InJsonLight_WithEntityAndNavigationProperty_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink
            {
                Url = CreateFakeUri()
            };
            IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().First();
            IEdmNavigationProperty navigationProperty =
                model.EntityContainers().Single().EntitySets().First().NavigationTargets.First().NavigationProperty;

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                Assert.DoesNotThrow(() => writer.WriteEntityReferenceLink(link, entitySet, navigationProperty));
            }
        }

        private static Uri CreateFakeUri()
        {
            return new Uri("aa:b");
        }

        private static ODataMessageWriterSettings CreateJsonLightSettings()
        {
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            settings.SetContentType(ODataFormat.Json);
            settings.SetMetadataDocumentUri(CreateFakeUri());
            return settings;
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
            orderType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo()
            {
                Name = "Customer",
                Target = customerType,
                TargetMultiplicity = EdmMultiplicity.One
            });

            var container = new EdmEntityContainer("Default", "Container");
            var orderSet = container.AddEntitySet("Orders", orderType);
            var customerSet = container.AddEntitySet("Customers", customerType);

            container.AddFunctionImport("GetId", new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), true));

            orderSet.AddNavigationTarget(orderType.NavigationProperties().Single(np => np.Name == "Customer"),
                customerSet);

            model.AddElement(container);
            return model;
        }

        private static IODataResponseMessage CreateResponse()
        {
            return new ODataMessageWrapper(Stream.Null);
        }
    }
}
