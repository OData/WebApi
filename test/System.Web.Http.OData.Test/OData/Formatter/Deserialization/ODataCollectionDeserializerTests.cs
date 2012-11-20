// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataCollectionDeserializerTests
    {
        private static IEdmModel _model = GetEdmModel();
        private static ODataDeserializerProvider _deserializerProvider = new DefaultODataDeserializerProvider(_model);
        private static IEdmTypeReference _addressType = _model.GetEdmTypeReference(typeof(Address)).AsComplex();
        private static IEdmCollectionTypeReference _addressCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(_addressType), isNullable: false);
        private static IEdmCollectionTypeReference _intCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(_model.GetEdmTypeReference(typeof(int))), isNullable: false);

        [Fact]
        public void Read_Roundtrip_ComplexCollection()
        {
            Address[] addresses = new[]
                {
                    new Address { City ="Redmond", ZipCode ="1", Street ="A", State ="123"},
                    new Address { City ="Seattle", ZipCode ="2", Street ="S", State ="321"}
                };
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(_addressCollectionType, new DefaultODataSerializerProvider(_model));
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(_addressCollectionType, _deserializerProvider);


            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            serializer.WriteObject(addresses, new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), _model), new ODataSerializerContext { RootElementName = "Property" });
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = deserializer.Read(new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model), new ODataDeserializerContext()) as IEnumerable;

            Assert.Equal(addresses, readAddresses.Cast<Address>(), new AddressComparer());
        }

        [Fact]
        public void Read_Roundtrip_PrimitiveCollection()
        {
            int[] numbers = Enumerable.Range(0, 100).ToArray();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(_intCollectionType, new DefaultODataSerializerProvider(_model));
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(_intCollectionType, _deserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            serializer.WriteObject(numbers, new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), _model), new ODataSerializerContext { RootElementName = "Property" });
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readnumbers = deserializer.Read(new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model), new ODataDeserializerContext()) as IEnumerable;

            Assert.Equal(numbers, readnumbers.Cast<int>());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            return builder.GetEdmModel();
        }

        private class AddressComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                return x.City == y.City && x.Country == y.Country && x.State == y.State && x.Street == y.Street && x.ZipCode == y.ZipCode;
            }

            public int GetHashCode(Address obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
