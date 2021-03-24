// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class ODataResourceSetDeserializerTest
    {
        private readonly IEdmModel _model;
        private readonly IEdmCollectionTypeReference _customersType;
        private readonly IEdmEntityTypeReference _customerType;
        private readonly ODataSerializerProvider _serializerProvider;
        private readonly ODataDeserializerProvider _deserializerProvider;

        public ODataResourceSetDeserializerTest()
        {
            _model = GetEdmModel();
            _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
            _customersType = new EdmCollectionTypeReference(new EdmCollectionType(_customerType));
            _serializerProvider = ODataSerializerProviderFactory.Create();
            _deserializerProvider = ODataDeserializerProviderFactory.Create();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataResourceSetDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            Assert.Null(deserializer.ReadInline(item: null, edmType: _customersType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ExceptionAssert.ThrowsArgument(
                () => deserializer.ReadInline(item: 42, edmType: _customersType, readContext: new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataResourceSetWrapperBase'.");
        }

        [Fact]
        public void ReadInline_Calls_ReadFeed()
        {
            // Arrange
            ODataDeserializerProvider deserializerProvider = _deserializerProvider;
            Mock<ODataResourceSetDeserializer> deserializer = new Mock<ODataResourceSetDeserializer>(deserializerProvider);
            ODataResourceSetWrapper feedWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            ODataDeserializerContext readContext = new ODataDeserializerContext();
            IEnumerable expectedResult = new object[0];

            deserializer.CallBase = true;
            deserializer.Setup(f => f.ReadResourceSet(feedWrapper, _customerType, readContext)).Returns(expectedResult).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(feedWrapper, _customersType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Same(expectedResult, result);
        }
  
        [Fact]
        public void ReadFeed_Throws_TypeCannotBeDeserialized()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataResourceSetWrapper feedWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns<ODataEdmTypeDeserializer>(null);

            ExceptionAssert.Throws<SerializationException>(
                () => deserializer.ReadResourceSet(feedWrapper, _customerType, readContext).GetEnumerator().MoveNext(),
                "'Microsoft.AspNet.OData.Test.Common.Models.Customer' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForEachEntry()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataResourceSetWrapper resourceSetWrapper = new ODataResourceSetWrapper(new ODataResourceSet());
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a2/") }));
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[0], _customerType, readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[1], _customerType, readContext)).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadResourceSet(resourceSetWrapper, _customerType, readContext);

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            entityDeserializer.Verify();
        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForDeltaFeeds()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataDeltaResourceSetWrapper resourceSetWrapper = new ODataDeltaResourceSetWrapper(new ODataDeltaResourceSet());
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a2/") }));
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = _model };

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[0], _customerType, readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[1], _customerType, readContext)).Returns("entry2").Verifiable();


            // Act
            var result = deserializer.ReadResourceSet(resourceSetWrapper, _customerType, readContext).Cast<object>().ToList();

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            entityDeserializer.Verify();

        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForDeltaFeeds_WithDeletes()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataDeltaResourceSetWrapper resourceSetWrapper = new ODataDeltaResourceSetWrapper(new ODataDeltaResourceSet());
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a2/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataDeletedResource {TypeName=typeof(Customer).FullName, Reason= DeltaDeletedEntryReason.Deleted, Id = new Uri("http://a2/"), Properties = new List<ODataProperty>() }));

            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = _model };

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[0], _customerType, readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[1], _customerType, readContext)).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadResourceSet(resourceSetWrapper, _customerType, readContext).Cast<object>().ToList();

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            var deleted = result[2] as EdmDeltaDeletedEntityObject;
            Assert.NotNull(deleted);
            Assert.Equal(typeof(Customer).FullName, deleted.ActualEdmType.FullTypeName());
            entityDeserializer.Verify();
        }

        [Fact]
        public void ReadFeed_Calls_ReadInlineForDeltaFeeds_WithMulipleDeltas()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            Mock<ODataEdmTypeDeserializer> entityDeserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Resource);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(deserializerProvider.Object);
            ODataDeltaResourceSetWrapper resourceSetWrapper = new ODataDeltaResourceSetWrapper(new ODataDeltaResourceSet { TypeName = typeof(Customer).FullName });
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a1/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataResource { Id = new Uri("http://a2/") }));
            resourceSetWrapper.Resources.Add(new ODataResourceWrapper(new ODataDeletedResource { TypeName = typeof(Customer).FullName, Reason = DeltaDeletedEntryReason.Deleted, Id = new Uri("http://a2/"), Properties = new List<ODataProperty>() }));

            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = _model };

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_customerType)).Returns(entityDeserializer.Object);
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[0], _customerType, readContext)).Returns("entry1").Verifiable();
            entityDeserializer.Setup(d => d.ReadInline(resourceSetWrapper.Resources[1], _customerType, readContext)).Returns("entry2").Verifiable();

            // Act
            var result = deserializer.ReadResourceSet(resourceSetWrapper, _customerType, readContext).Cast<object>().ToList();

            // Assert
            Assert.Equal(new[] { "entry1", "entry2" }, result.OfType<String>());
            var deleted = result[2] as EdmDeltaDeletedEntityObject;
            Assert.NotNull(deleted);
            Assert.Equal(typeof(Customer).FullName, deleted.ActualEdmType.FullTypeName());

            var link = result[3] as EdmDeltaLink;
            Assert.NotNull(link);
            Assert.Equal(typeof(Customer).FullName, link.ActualEdmType.FullTypeName());
            Assert.Equal("TestRelation", link.Relationship);

            var dellink = result[4] as EdmDeltaDeletedLink;
            Assert.NotNull(dellink);
            Assert.Equal(typeof(Customer).FullName, dellink.ActualEdmType.FullTypeName());
            Assert.Equal("TestDeletedRel", dellink.Relationship);

            entityDeserializer.Verify();
        }

        [Fact]
        public async Task Read_ReturnsEdmComplexObjectCollection_TypelessMode()
        {
            // Arrange
            IEdmTypeReference addressType = _model.GetEdmTypeReference(typeof(Address)).AsComplex();
            IEdmCollectionTypeReference addressCollectionType =
                new EdmCollectionTypeReference(new EdmCollectionType(addressType));

            HttpContent content = new StringContent("{ 'value': [ {'@odata.type':'Microsoft.AspNet.OData.Test.Common.Models.Address', 'City' : 'Redmond' } ] }");
            var headers = FormatterTestHelper.GetContentHeaders("application/json");
            IODataRequestMessage request = ODataMessageWrapperHelper.Create(await content.ReadAsStreamAsync(), headers);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), _model);
            var deserializer = new ODataResourceSetDeserializer(_deserializerProvider);
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model,
                ResourceType = typeof(IEdmObject),
                ResourceEdmType = addressCollectionType
            };

            // Act
            IEnumerable result = deserializer.Read(reader, typeof(IEdmObject), readContext) as IEnumerable;

            // Assert
            var addresses = result.Cast<EdmComplexObject>();
            Assert.NotNull(addresses);

            EdmComplexObject address = Assert.Single(addresses);
            Assert.Equal(new[] {"City"}, address.GetChangedPropertyNames());

            object city;
            Assert.True(address.TryGetPropertyValue("City", out city));
            Assert.Equal("Redmond", city);
        }

        [Fact]
        public void Read_Roundtrip_ComplexCollection()
        {
            // Arrange
            Address[] addresses = new[]
                {
                    new Address { City ="Redmond", StreetAddress ="A", State ="123"},
                    new Address { City ="Seattle", StreetAddress ="S", State ="321"}
                };

            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ODataResourceSetDeserializer deserializer = new ODataResourceSetDeserializer(_deserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, _model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = _model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _model };

            // Act
            serializer.WriteObject(addresses, addresses.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = deserializer.Read(messageReader, typeof(Address[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(addresses, readAddresses.Cast<Address>(), new AddressComparer());
        }

        private class AddressComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                return x.City == y.City && x.State == y.State && x.StreetAddress == y.StreetAddress && x.ZipCode == y.ZipCode;
            }

            public int GetHashCode(Address obj)
            {
                throw new NotImplementedException();
            }
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("customers");
            builder.ComplexType<Address>();
            return builder.GetEdmModel();
        }
    }
}
