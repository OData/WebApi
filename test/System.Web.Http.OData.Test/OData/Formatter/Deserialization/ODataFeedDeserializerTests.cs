// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataFeedDeserializerTests
    {
        private static IEdmModel _model = GetEdmModel();
        private static ODataDeserializerProvider _deserializerProvider = new DefaultODataDeserializerProvider(_model);
        private static IEdmTypeReference _customerType = _model.GetEdmTypeReference(typeof(Customer)).AsEntity();
        private static IEdmCollectionTypeReference _customersCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(_customerType), isNullable: false);

        ODataFeedDeserializer _instance = new ODataFeedDeserializer(_customersCollectionType, _deserializerProvider);

        [Fact]
        public void Read_Roundtrip()
        {
            Customer[] customers = new[]
                {
                    new Customer { ID =1, FirstName = "A", LastName = "1" },
                    new Customer { ID =2, FirstName = "B", LastName = "2" },
                };
            ODataFeedSerializer serializer = new ODataFeedSerializer(_customersCollectionType, new DefaultODataSerializerProvider(_model));

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            serializer.WriteObject(customers, new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), _model), new ODataSerializerContext());
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable<Customer> readCustomers = _instance.Read(new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model), new ODataDeserializerContext()) as IEnumerable<Customer>;

            Assert.Equal(customers, readCustomers, new CustomerComparer());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        private class CustomerComparer : IEqualityComparer<Customer>
        {
            public bool Equals(Customer x, Customer y)
            {
                return x.ID == y.ID && x.FirstName == y.FirstName && x.LastName == y.LastName;
            }

            public int GetHashCode(Customer obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
