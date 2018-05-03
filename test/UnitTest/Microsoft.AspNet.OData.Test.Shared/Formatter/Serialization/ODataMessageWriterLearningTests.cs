﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
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
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataCollectionWriter());
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
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataCollectionWriter(itemTypeReference));
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
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataResourceWriter());
            }
        }

        [Fact]
        public void TestCreateODataEntryWriter_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataResourceWriter(entitySet));
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
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataResourceSetWriter());
            }
        }

        [Fact]
        public void TestCreateODataFeedWriter_InJsonLight_WithEntitySetButWithoutType_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => writer.CreateODataResourceSetWriter(entitySet));
            }
        }

        [Fact]
        public void TestWriteEntityReferenceLink_InJsonLight_WithoutEntitySetOrNavigationProperty_DoesNotThrow()
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
                ExceptionAssert.DoesNotThrow(() => writer.WriteEntityReferenceLink(link));
            }
        }

        [Fact]
        public void TestWriteEntityReferenceLink_InJsonLight_WithEntitySetButNotNavigationProperty_DoesNotThrow()
        {
            // Arrange
            IODataResponseMessage response = CreateResponse();
            ODataMessageWriterSettings settings = CreateJsonLightSettings();
            IEdmModel model = CreateModel();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink
            {
                Url = CreateFakeUri()
            };
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => writer.WriteEntityReferenceLink(link));
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
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().First();
            Assert.NotNull(entitySet);
            IEdmNavigationProperty navigationProperty =
                model.EntityContainer.EntitySets().First().NavigationPropertyBindings.First().NavigationProperty;
            Assert.NotNull(navigationProperty);

            using (ODataMessageWriter writer = new ODataMessageWriter(response, settings, model))
            {
                // Act & Assert
                ExceptionAssert.DoesNotThrow(() => writer.WriteEntityReferenceLink(link));
            }
        }

        private static Uri CreateFakeUri()
        {
            return new Uri("aa:b");
        }

        private static ODataMessageWriterSettings CreateJsonLightSettings()
        {
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);
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

            container.AddFunctionImport(
                new EdmFunction(
                    "Default",
                    "GetId",
                    new EdmPrimitiveTypeReference(
                        EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32),
                        isNullable: true)));

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
