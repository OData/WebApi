// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataComplexTypeSerializerTests
    {
        IEdmModel _model;
        Address _address;
        ODataComplexTypeSerializer _serializer;
        IEdmComplexType _addressType;

        public ODataComplexTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _address = new Address()
            {
                Street = "One Microsoft Way",
                City = "Redmond",
                State = "Washington",
                Country = "United States",
                ZipCode = "98052"
            };

            _addressType = _model.FindDeclaredType("Default.Address") as IEdmComplexType;

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider(_model);
            _serializer = new ODataComplexTypeSerializer(new EdmComplexTypeReference(_addressType, isNullable: false), serializerProvider);
        }

        [Fact]
        public void CreateProperty_WritesAllDeclaredProperties()
        {
            var property = _serializer.CreateProperty(_address, "ComplexElement", new ODataSerializerContext());

            Assert.Equal("ComplexElement", property.Name);
            ODataComplexValue complexValue = Assert.IsType<ODataComplexValue>(property.Value);

            Assert.Equal(complexValue.TypeName, "Default.Address");
            Assert.Equal(
                complexValue.Properties.Select(p => Tuple.Create(p.Name, p.Value as string)),
                new[] { 
                    Tuple.Create("Street","One Microsoft Way"), 
                    Tuple.Create("City","Redmond"),
                    Tuple.Create("State","Washington"),
                    Tuple.Create("Country", "United States"),
                    Tuple.Create("ZipCode","98052") });
        }
    }
}
