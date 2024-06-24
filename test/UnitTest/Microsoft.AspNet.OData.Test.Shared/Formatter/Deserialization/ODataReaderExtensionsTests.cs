//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public partial class ODataReaderExtensionsTests
    {
        private static readonly EdmModel Model;

        static ODataReaderExtensionsTests()
        {
            Model = new EdmModel();

            // Address
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmCoreModel.Instance.GetString(false));
            Model.AddElement(address);

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            Model.AddElement(customer);
            customer.AddKeys(customer.AddStructuralProperty("CustomerID", EdmCoreModel.Instance.GetInt32(false)));
            customer.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(true));
            customer.AddStructuralProperty("Location", new EdmComplexTypeReference(address, false));

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order");
            Model.AddElement(order);
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmCoreModel.Instance.GetInt32(false)));
            order.AddStructuralProperty("Price", EdmCoreModel.Instance.GetInt32(false));

            // VipOrder
            EdmEntityType vipOrder = new EdmEntityType("NS", "VipOrder", order);
            Model.AddElement(vipOrder);
            vipOrder.AddKeys(vipOrder.AddStructuralProperty("Email", EdmCoreModel.Instance.GetString(false)));

            var orderNav = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Order",
                    Target = order,
                    TargetMultiplicity = EdmMultiplicity.ZeroOrOne
                });

            var ordresNav = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    Target = order,
                    TargetMultiplicity = EdmMultiplicity.Many
                });

            EdmEntityContainer defaultContainer = new EdmEntityContainer("NS", "Container");
            Model.AddElement(defaultContainer);
            EdmEntitySet customers = defaultContainer.AddEntitySet("Customers", customer);
            EdmEntitySet orders = defaultContainer.AddEntitySet("Orders", order);
            customers.AddNavigationTarget(orderNav, orders);
            customers.AddNavigationTarget(ordresNav, orders);
        }

        [Fact]
        public void ReadResourceOrResourceSet_ThrowsArgumentNull_EdmType()
        {
            // Arrange & Act & Assert
            ODataReader reader = null;
            ExceptionAssert.ThrowsArgumentNull(() => reader.ReadResourceOrResourceSet(), "reader");
        }

        [Fact]
        public async Task ReadResourceOrResourceSetAsync_ThrowsArgumentNull_EdmType()
        {
            // Arrange & Act & Assert
            ODataReader reader = null;
            await ExceptionAssert.ThrowsArgumentNullAsync(() => reader.ReadResourceOrResourceSetAsync(), "reader");
        }

        [Fact]
        public async Task ReadResourceWorksAsExpected()
        {
            // Arrange
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                "\"Location\": { \"Street\":\"154TH AVE\"}," +
                "\"Order\": {\"OrderId\": 8, \"Price\": 82 }," +
                "\"Orders\": []" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemBase item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);
            Assert.Equal(3, resource.NestedResourceInfos.Count);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        [Fact]
        public async Task ReadResourceWithPropertyWithoutValueButWithInstanceAnnotationsWorksAsExpected()
        {
            // Arrange
            // Property 'Name' without value but with instance annotations
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 17," +
                "\"Name@Custom.PrimitiveAnnotation\":123," +
                "\"Name@Custom.BooleanAnnotation\":true," +
                "\"Location\": { \"Street\":\"154TH AVE\"}" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemBase item = await ReadPayloadAsync(payload, Model, func, ODataVersion.V4, false, "*");

            // Assert
            Assert.NotNull(item);
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);
            Assert.NotNull(resource.ResourceBase);
            ODataProperty customerIdProp = Assert.Single(resource.ResourceBase.Properties);
            Assert.Equal("CustomerID", customerIdProp.Name);
            Assert.Equal(17, customerIdProp.Value);

            ODataPropertyInfo nameProp = Assert.Single(resource.NestedPropertyInfos);
            Assert.Equal("Name", nameProp.Name);
            Assert.Equal(2, nameProp.InstanceAnnotations.Count);

            ODataInstanceAnnotation primitiveAnnotation = nameProp.InstanceAnnotations.First(i => i.Name == "Custom.PrimitiveAnnotation");
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(primitiveAnnotation.Value);
            Assert.Equal(123, primitiveValue.Value);

            ODataInstanceAnnotation booleanAnnotation = nameProp.InstanceAnnotations.First(i => i.Name == "Custom.BooleanAnnotation");
            ODataPrimitiveValue booleanValue = Assert.IsType<ODataPrimitiveValue>(booleanAnnotation.Value);
            Assert.True((bool)booleanValue.Value);

            ODataNestedResourceInfoWrapper nestedInfoWrapper = Assert.Single(resource.NestedResourceInfos);
            Assert.Equal("Location", nestedInfoWrapper.NestedResourceInfo.Name);
        }

        [Fact]
        public async Task ReadResourceSetWorksAsExpected()
        {
            // Arrange
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers\"," +
                "\"value\": [" +
                 "{" +
                    "\"CustomerID\": 7," +
                    "\"Location\": { \"Street\":\"154TH AVE\"}," +
                    "\"Order\": {\"OrderId\": 8, \"Price\": 82 }," +
                    "\"Orders\": []" +
                   "}" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemBase item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceSetWrapper resourceSet = Assert.IsType<ODataResourceSetWrapper>(item);
            ODataResourceWrapper resource = Assert.Single(resourceSet.Resources);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));
        }

        [Fact]
        public async Task ReadResourceSetWithNestedResourceSetWorksAsExpected()
        {
            // Arrange
            const string payload =
            "{" +
                "\"@odata.context\":\"http://localhost/$metadata#Customers\"," +
                "\"value\": [" +
                 "{" +
                    "\"CustomerID\": 7," +
                    "\"Location\": { \"Street\":\"154TH AVE\"}," +
                    "\"Order\": {\"OrderId\": 8, \"Price\": 82 }," +
                    "\"Orders\": [" +
                        "{\"OrderId\": 8, \"Price\": 82 }," +
                        "{\"@odata.type\": \"#NS.VipOrder\",\"OrderId\": 9, \"Price\": 42, \"Email\": \"abc@efg.com\" }" +
                      "]" +
                   "}" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceSetReaderAsync(customers, customers.EntityType());
            ODataItemBase item = await ReadPayloadAsync(payload, Model, func);

            // Assert
            Assert.NotNull(item);
            ODataResourceSetWrapper resourceSet = Assert.IsType<ODataResourceSetWrapper>(item);
            ODataResourceWrapper resource = Assert.Single(resourceSet.Resources);
            Assert.Equal(new[] { "Location", "Order", "Orders" }, resource.NestedResourceInfos.Select(n => n.NestedResourceInfo.Name));

            ODataNestedResourceInfoWrapper orders = resource.NestedResourceInfos.First(n => n.NestedResourceInfo.Name == "Orders");
            ODataItemBase nestedItem = Assert.Single(orders.NestedItems);

            ODataResourceSetWrapper ordersSet = Assert.IsType<ODataResourceSetWrapper>(nestedItem);
            Assert.Equal(2, ordersSet.Resources.Count);
            Assert.Collection(ordersSet.Resources,
                r =>
                {
                    Assert.Equal("NS.Order", r.ResourceBase.TypeName);
                    Assert.Equal(82, r.ResourceBase.Properties.First(p => p.Name == "Price").Value);
                },
                r =>
                {
                    Assert.Equal("NS.VipOrder", r.ResourceBase.TypeName);
                    Assert.Equal("abc@efg.com", r.ResourceBase.Properties.First(p => p.Name == "Email").Value);
                });
        }

        [Fact]
        public async Task ReadEntityReferenceLinksSetWorksAsExpected_V401()
        {
            // Arrange
            string payload = "{" + // -> ResourceStart
                "\"@odata.context\":\"http://localhost/$metadata#Customers/$entity\"," +
                "\"CustomerID\": 7," +
                "\"Orders\":[" +  // -> NestedResourceInfoStart
                    "{ \"@id\": \"http://svc/Orders(2)\" }," +
                    "{ \"@id\": \"http://svc/Orders(3)\" }," +
                    "{ \"@id\": \"http://svc/Orders(4)\" }" +
                "]" +
            "}";

            IEdmEntitySet customers = Model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // Guard

            // Act
            Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(customers, customers.EntityType());
            ODataItemBase item = await ReadPayloadAsync(payload, Model, func, ODataVersion.V401);

            // Assert
            Assert.NotNull(item);

            // --- Resource
            //     |--- NestedResourceInfo
            //        |--- NestedResourceSet
            //              |--- Resource (2)
            //              |--- Resource (3)
            //              |--- Resource (4)
            ODataResourceWrapper resource = Assert.IsType<ODataResourceWrapper>(item);

            ODataNestedResourceInfoWrapper nestedResourceInfo = Assert.Single(resource.NestedResourceInfos);
            Assert.Equal("Orders", nestedResourceInfo.NestedResourceInfo.Name);
            Assert.True(nestedResourceInfo.NestedResourceInfo.IsCollection);

            ODataResourceSetWrapper ordersResourceSet = Assert.IsType<ODataResourceSetWrapper>(Assert.Single(nestedResourceInfo.NestedItems));
            Assert.Equal(3, ordersResourceSet.Resources.Count);
            Assert.Collection(ordersResourceSet.Resources,
                r =>
                {
                    Assert.Equal("http://svc/Orders(2)", r.ResourceBase.Id.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(3)", r.ResourceBase.Id.OriginalString);
                },
                r =>
                {
                    Assert.Equal("http://svc/Orders(4)", r.ResourceBase.Id.OriginalString);
                });
        }

        private async Task<ODataItemBase> ReadPayloadAsync(string payload,
            IEdmModel edmModel, Func<ODataMessageReader, Task<ODataReader>> createReader, ODataVersion version = ODataVersion.V4,
            bool readUntypedAsString = false,
            string annotationFilter = null)
        {
            //var message = new InMemoryMessage()
            //{
            //    Stream = new MemoryStream(Encoding.UTF8.GetBytes(payload))
            //};
            ODataMessageWrapper message = new ODataMessageWrapper(new MemoryStream(Encoding.UTF8.GetBytes(payload)));
            message.SetHeader("Content-Type", "application/json;odata.metadata=minimal");

            ODataMessageReaderSettings readerSettings = new ODataMessageReaderSettings()
            {
                BaseUri = new Uri("http://localhost/$metadata"),
                EnableMessageStreamDisposal = true,
                ReadUntypedAsString = readUntypedAsString,
                Version = version,
            };

            if (annotationFilter != null)
            {
                readerSettings.ShouldIncludeAnnotation = ODataUtils.CreateAnnotationFilter(annotationFilter);
            }

            using (var msgReader = new ODataMessageReader((IODataRequestMessageAsync)message, readerSettings, edmModel))
            {
                ODataReader reader = await createReader(msgReader);
                return await reader.ReadResourceOrResourceSetAsync();
            }
        }
    }
}
