// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntityDeserializerTests
    {
        private IEdmModel _edmModel = EdmTestHelpers.GetModel();
        private ODataDeserializerContext _readContext = new ODataDeserializerContext();
        private IEdmEntityTypeReference _productEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(Product)).AsEntity();
        private IEdmEntityTypeReference _supplierEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(Supplier)).AsEntity();
        private ODataDeserializerProvider _deserializerProvider = new DefaultODataDeserializerProvider(EdmTestHelpers.GetModel());

        [Fact]
        public void ReadFromStreamAsync()
        {
            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_productEdmType, _deserializerProvider);
            Product product = deserializer.Read(GetODataMessageReader(GetODataMessage(BaselineResource.ProductInsertData), _edmModel), _readContext) as Product;

            Assert.Equal(product.ID, 0);
            Assert.Equal(product.Rating, 4);
            Assert.Equal(product.Price, 2.5m);
            Assert.Equal(product.ReleaseDate, new DateTime(1992, 1, 1, 0, 0, 0));
            Assert.Null(product.DiscontinuedDate);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData()
        {
            IEdmEntityType supplierEntityType = EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_supplierEdmType, _deserializerProvider);
            Supplier supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(BaselineResource.SuppliersInsertData), _edmModel), _readContext) as Supplier;

            Assert.Equal(supplier.Name, "Supplier Name");

            Assert.NotNull(supplier.Products);
            Assert.Equal(6, supplier.Products.Count);
            Assert.Equal("soda", supplier.Products.ToList()[1].Name);

            Assert.NotNull(supplier.Address);
            Assert.Equal("Supplier City", supplier.Address.City);
            Assert.Equal("123456", supplier.Address.ZipCode);
        }

        [Fact]
        public void Read_PatchMode()
        {
            IEdmEntityType supplierEntityType = EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;
            _readContext.IsPatchMode = true;
            _readContext.PatchEntityType = typeof(Delta<Supplier>);

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_supplierEdmType, _deserializerProvider);
            Delta<Supplier> supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(BaselineResource.SuppliersPatchData), _edmModel), _readContext) as Delta<Supplier>;

            Assert.NotNull(supplier);
            Assert.Equal(supplier.GetChangedPropertyNames(), new string[] { "Name", "Address" });

            Assert.Equal((supplier as dynamic).Name, "Supplier Name");
            Assert.Equal("Supplier City", (supplier as dynamic).Address.City);
            Assert.Equal("123456", (supplier as dynamic).Address.ZipCode);
        }

        [Fact]
        public void Read_Throws_On_UnknownEntityType()
        {
            IEdmEntityType supplierEntityType = EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_productEdmType, _deserializerProvider);

            Assert.Throws<ODataException>(
                () => deserializer.Read(GetODataMessageReader(GetODataMessage(BaselineResource.SuppliersInsertData), _edmModel), _readContext),
                "An entry with type 'ODataDemo.Supplier' was found, but it is not assignable to the expected type 'ODataDemo.Product'. The type specified in the entry must be equal to either the expected type or a derived type.");
        }

        private static Type EdmTypeResolver(IEdmTypeReference edmType)
        {
            return Type.GetType(edmType.FullName());
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/OData/OData.svc/Products");

            request.Content = new StringContent(content);
            request.Headers.Add("DataServiceVersion", "1.0");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");

            return new HttpRequestODataMessage(request);
        }

        public class Product
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public DateTime? ReleaseDate { get; set; }

            public DateTime? DiscontinuedDate { get; set; }

            public int Rating { get; set; }

            public decimal Price { get; set; }

            public virtual Category Category { get; set; }

            public virtual Supplier Supplier { get; set; }
        }

        public class Category
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public virtual ICollection<Product> Products { get; set; }
        }

        public class Supplier
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public Address Address { get; set; }

            public int Concurrency { get; set; }

            public SupplierRating SupplierRating { get; set; }

            public virtual ICollection<Product> Products { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string ZipCode { get; set; }

            public string Country { get; set; }
        }

        public enum SupplierRating
        {
            Gold,
            Silver,
            Bronze
        }
    }
}
