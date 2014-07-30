// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataRawValueMediaTypeMappingTests
    {
        [Fact]
        public void TryMatchMediaTypeWithPrimitiveRawValueThrowsArgumentNullWhenRequestIsNull()
        {
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();

            Assert.ThrowsArgumentNull(() => { mapping.TryMatchMediaType(null); }, "request");
        }

        [Fact]
        public void TryMatchMediaTypeWithPrimitiveRawValueMatchesRequest()
        {
            IEdmModel model = ODataTestUtil.GetEdmModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(FormatterPerson)) as IEdmEntityType).FindProperty("Age"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("People"), new KeyValuePathSegment("1"), propertySegment, new ValuePathSegment());
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(1.0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithNonODataRequestDoesntMatchRequest()
        {
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithNonRawvalueRequestDoesntMatchRequest()
        {
            IEdmModel model = ODataTestUtil.GetEdmModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(FormatterPerson)) as IEdmEntityType).FindProperty("Age"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("People"), new KeyValuePathSegment("1"), propertySegment);
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithBinaryRawValueMatchesRequest()
        {
            IEdmModel model = GetBinaryModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(RawValueEntity)) as IEdmEntityType).FindProperty("BinaryProperty"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("RawValue"), new KeyValuePathSegment("1"), propertySegment, new ValuePathSegment());
            ODataBinaryValueMediaTypeMapping mapping = new ODataBinaryValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RawValue(1)/BinaryProperty/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(1.0, mapResult);
        }

        private class RawValueEntity
        {
            public int Id { get; set; }
            public byte[] BinaryProperty { get; set; }
        }

        private static IEdmModel GetBinaryModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<RawValueEntity>("RawValue");
            return builder.GetEdmModel();
        }
    }
}
