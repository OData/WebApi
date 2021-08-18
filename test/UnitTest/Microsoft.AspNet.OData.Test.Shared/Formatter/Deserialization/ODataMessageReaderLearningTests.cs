//-----------------------------------------------------------------------------
// <copyright file="ODataMessageReaderLearningTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataMessageReaderLearningTests
    {
        [Fact]
        public void TestCreateODataCollectionReader_WithoutTypeReference_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.Throws<ODataException>(() => reader.CreateODataCollectionReader());
            }
        }

        [Fact]
        public void TestCreateODataCollectionReader_WithTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmTypeReference expectedItemTypeReference = new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataCollectionReader(expectedItemTypeReference));
            }
        }

        [Fact]
        public void TestCreateODataResourceReader_WithoutEntitySetOrType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceReader());
            }
        }

        [Fact]
        public void TestCreateODataResourceReader_WithEntityTypeButWithoutSet_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntityType entityType = model.EntityContainer.EntitySets().First().EntityType();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.Throws<ODataException>(() => reader.CreateODataResourceReader(null, entityType));
            }
        }

        [Fact]
        public void TestCreateODataResourceReader_WithComplexTypeButWithoutSet_DoesnotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmComplexType complexType = model.SchemaElements.OfType<IEdmComplexType>().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceReader(null, complexType));
            }
        }

        [Fact]
        public void TestCreateODataResourceReader_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceReader(entitySet, null));
            }
        }

        [Fact]
        public void TestCreateODataResourceSetReader_WithoutEntitySetOrType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceSetReader());
            }
        }

        [Fact]
        public void TestCreateODataResourceSetReader_WithEntityTypeButWithoutSet_Throws()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntityType entityType = model.EntityContainer.EntitySets().First().EntityType();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.Throws<ODataException>(() => reader.CreateODataResourceSetReader(entityType));
            }
        }

        [Fact]
        public void TestCreateODataResourceSetReader_WithComplexTypeButWithoutSet_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmComplexType complexType = model.SchemaElements.OfType<IEdmComplexType>().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceSetReader(complexType));
            }
        }

        [Fact]
        public void TestCreateODataResourceSetReader_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = CreateRequest();
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.CreateODataResourceSetReader(entitySet, null));
            }
        }

        [Fact]
        public async Task TestReadEntityReferenceLink_WithoutNavigationProperty_Throws()
        {
            // Arrange
            IODataRequestMessage request = await CreateRequest("{\"odata.id\":\"aa:b\"}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.Throws<ODataException>(() => reader.ReadEntityReferenceLink());
            }
        }

        [Fact]
        public async Task TestReadEntityReferenceLink_WithNavigationProperty_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = await CreateRequest("{\"@odata.id\":\"aa:b\"}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.ReadEntityReferenceLink());
            }
        }

        [Fact]
        public async Task TestReadProperty_WithoutStructuralPropertyOrTypeReference_DoesNotThrows()
        {
            // Arrange
            IODataRequestMessage request = await CreateRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.ReadProperty());
            }
        }

        [Fact]
        public async Task TestReadProperty_WithStructuralProperty_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = await CreateRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmStructuralProperty property = model.EntityContainer.EntitySets().First().EntityType().StructuralProperties().First();

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.ReadProperty(property));
            }
        }

        [Fact]
        public async Task TestReadProperty_WithTypeReference_DoesNotThrow()
        {
            // Arrange
            IODataRequestMessage request = await CreateRequest("{\"value\":1}");
            ODataMessageReaderSettings settings = CreateSettings();
            IEdmModel model = CreateModel();
            IEdmTypeReference expectedPropertyTypeReference = new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            using (ODataMessageReader reader = new ODataMessageReader(request, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => reader.ReadProperty(expectedPropertyTypeReference));
            }
        }

        private static IODataRequestMessage CreateRequest()
        {
            //HttpContentHeaders headers;
            //using (HttpContent content = new StreamContent(Stream.Null))
            //{
            //    headers = content.Headers;
            //}

            var headers = FormatterTestHelper.GetContentHeaders("application/json;odata.metadata=full");
            return ODataMessageWrapperHelper.Create(Stream.Null, headers);
        }

        private static async Task<IODataRequestMessage> CreateRequest(string body)
        {
            HttpContent content = new StringContent(body);
            var headers = FormatterTestHelper.GetContentHeaders("application/json;odata.metadata=full");

            return ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headers);
        }

        private static IEdmModel CreateModel()
        {
            var model = new EdmModel();

            var addressType = new EdmComplexType("Default", "Address");
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(addressType);

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

            container.AddFunctionImport(
                new EdmFunction(
                    "Default",
                    "GetIDs",
                    new EdmCollectionTypeReference(
                        new EdmCollectionType(
                            new EdmPrimitiveTypeReference(
                                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32),
                                isNullable: false)))));

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
