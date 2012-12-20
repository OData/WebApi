// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntityDeserializerTests
    {
        private readonly IEdmModel _edmModel;
        private readonly ODataDeserializerContext _readContext;
        private readonly IEdmEntityTypeReference _productEdmType;
        private readonly IEdmEntityTypeReference _supplierEdmType;
        private readonly ODataDeserializerProvider _deserializerProvider;

        public ODataEntityDeserializerTests()
        {
            _edmModel = EdmTestHelpers.GetModel();
            IEdmEntitySet entitySet = _edmModel.EntityContainers().Single().FindEntitySet("Products");
            _readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new EntitySetPathSegment(entitySet))
            };
            _productEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(Product)).AsEntity();
            _supplierEdmType = EdmTestHelpers.GetModel().GetEdmTypeReference(typeof(Supplier)).AsEntity();
            _deserializerProvider = new DefaultODataDeserializerProvider(EdmTestHelpers.GetModel());
        }

        [Fact]
        public void ReadFromStreamAsync_ForJsonLight()
        {
            ReadFromStreamAsync(BaselineResource.ProductRequestEntryInPlainOldJson, true);
        }

        [Fact]
        public void ReadFromStreamAsync_ForAtom()
        {
            ReadFromStreamAsync(BaselineResource.ProductRequestEntryInAtom, false);
        }

        private void ReadFromStreamAsync(string content, bool json)
        {
            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_productEdmType, _deserializerProvider);
            Product product = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                _readContext) as Product;

            Assert.Equal(product.ID, 0);
            Assert.Equal(product.Rating, 4);
            Assert.Equal(product.Price, 2.5m);
            Assert.Equal(product.ReleaseDate, new DateTime(1992, 1, 1, 0, 0, 0));
            Assert.Null(product.DiscontinuedDate);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData_ForJsonLight()
        {
            ReadFromStreamAsync_ComplexTypeAndInlineData(BaselineResource.SupplierRequestEntryInPlainOldJson, true);
        }

        [Fact]
        public void ReadFromStreamAsync_ComplexTypeAndInlineData_ForAtom()
        {
            ReadFromStreamAsync_ComplexTypeAndInlineData(BaselineResource.SupplierRequestEntryInAtom, false);
        }

        private void ReadFromStreamAsync_ComplexTypeAndInlineData(string content, bool json)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_supplierEdmType, _deserializerProvider);
            Supplier supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json), _edmModel),
                _readContext) as Supplier;

            Assert.Equal(supplier.Name, "Supplier Name");

            Assert.NotNull(supplier.Products);
            Assert.Equal(6, supplier.Products.Count);
            Assert.Equal("soda", supplier.Products.ToList()[1].Name);

            Assert.NotNull(supplier.Address);
            Assert.Equal("Supplier City", supplier.Address.City);
            Assert.Equal("123456", supplier.Address.ZipCode);
        }

        [Fact]
        public void Read_PatchMode_ForJsonLight()
        {
            Read_PatchMode(BaselineResource.SupplierPatchInPlainOldJson, true);
        }

        [Fact]
        public void Read_PatchMode_ForAtom()
        {
            Read_PatchMode(BaselineResource.SupplierPatchInAtom, false);
        }

        private void Read_PatchMode(string content, bool json)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;
            _readContext.IsPatchMode = true;
            _readContext.PatchEntityType = typeof(Delta<Supplier>);

            ODataEntityDeserializer deserializer =
                new ODataEntityDeserializer(_supplierEdmType, _deserializerProvider);
            Delta<Supplier> supplier = deserializer.Read(GetODataMessageReader(GetODataMessage(content, json),
                _edmModel), _readContext) as Delta<Supplier>;

            Assert.NotNull(supplier);
            Assert.Equal(supplier.GetChangedPropertyNames(), new string[] { "ID", "Name", "Address" });

            Assert.Equal((supplier as dynamic).Name, "Supplier Name");
            Assert.Equal("Supplier City", (supplier as dynamic).Address.City);
            Assert.Equal("123456", (supplier as dynamic).Address.ZipCode);
        }

        [Fact]
        public void Read_ThrowsOnUnknownEntityType_ForJsonLight()
        {
            Read_ThrowsOnUnknownEntityType(BaselineResource.SupplierRequestEntryInPlainOldJson, true,
                "The property 'Concurrency' does not exist on type 'ODataDemo.Product'. Make sure to only use property names that are defined by the type.");
        }

        [Fact]
        public void Read_ThrowsOnUnknownEntityType_ForAtom()
        {
            Read_ThrowsOnUnknownEntityType(BaselineResource.SupplierRequestEntryInAtom, false,
                "An entry with type 'ODataDemo.Supplier' was found, but it is not assignable to the expected type 'ODataDemo.Product'. The type specified in the entry must be equal to either the expected type or a derived type.");
        }
        
        private void Read_ThrowsOnUnknownEntityType(string content, bool json, string expectedMessage)
        {
            IEdmEntityType supplierEntityType =
                EdmTestHelpers.GetModel().FindType("ODataDemo.Supplier") as IEdmEntityType;

            ODataEntityDeserializer deserializer = new ODataEntityDeserializer(_productEdmType, _deserializerProvider);

            Assert.Throws<ODataException>(() => deserializer.Read(GetODataMessageReader(GetODataMessage(content, json),
                _edmModel), _readContext), expectedMessage);
        }

        private static Type EdmTypeResolver(IEdmTypeReference edmType)
        {
            return Type.GetType(edmType.FullName());
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content, bool json)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/OData/OData.svc/Products");

            request.Content = new StringContent(content);
            request.Headers.Add("DataServiceVersion", "1.0");

            if (json)
            {
                MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
                mediaType.Parameters.Add(new NameValueHeaderValue("odata", "fullmetadata"));
                request.Headers.Accept.Add(mediaType);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");
            }

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
