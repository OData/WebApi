﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class EnumSerializerTest
    {
        [Fact]
        public void GetEdmTypeSerializer_ReturnODataEnumSerializer_ForEnumType()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEnumTypeReference(new EdmEnumType("TestModel", "Color"), isNullable: false);

            // Act
            ODataEdmTypeSerializer serializer = new DefaultODataSerializerProvider().GetEdmTypeSerializer(edmType);

            // Assert
            Assert.NotNull(serializer);
            Assert.IsType<ODataEnumSerializer>(serializer);
        }

        [Fact]
        public void WriteObject_Throws_ForNullMessageWriterParameter()
        {
            // Arrange
            object graph = null;
            Type type = typeof(Color);
            ODataMessageWriter messageWriter = null;
            ODataSerializerContext writeContext = new ODataSerializerContext();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataEnumSerializer().WriteObject(graph, type, messageWriter, writeContext),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ForNullWriteContextParameter()
        {
            // Arrange
            object graph = null;
            Type type = typeof(Color);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            httpRequestMessage.Content = new StringContent("");
            ODataMessageWriter messageWriter = new ODataMessageWriter(new HttpRequestODataMessage(httpRequestMessage));
            ODataSerializerContext writeContext = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ODataEnumSerializer().WriteObject(graph, type, messageWriter, writeContext),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Throws_ForWriteContextWithoutRootElementName()
        {
            // Arrange
            object graph = null;
            Type type = typeof(Color);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            httpRequestMessage.Content = new StringContent("");
            ODataMessageWriter messageWriter = new ODataMessageWriter(new HttpRequestODataMessage(httpRequestMessage));
            ODataSerializerContext writeContext = new ODataSerializerContext();

            // Act & Assert
            Assert.ThrowsArgument(
                () => new ODataEnumSerializer().WriteObject(graph, type, messageWriter, writeContext),
                "writeContext",
                "The 'RootElementName' property is required on 'ODataSerializerContext'.");
        }

        [Fact]
        public void CreateODataValue_Throws_ForNonEnumType()
        {
            // Arrange
            object graph = null;
            IEdmTypeReference expectedType = EdmCoreModel.Instance.GetInt32(false);
            ODataSerializerContext writeContext = new ODataSerializerContext();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => new ODataEnumSerializer().CreateODataValue(graph, expectedType, writeContext),
                "ODataEnumSerializer cannot write an object of type 'Edm.Int32'.");
        }

        [Fact]
        public void EnumTypeSerializerTestForOData()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = GetFormatter();
            ObjectContent<EnumComplex> content = new ObjectContent<EnumComplex>(
                new EnumComplex()
                {
                    RequiredColor = Color.Red | Color.Blue,
                    NullableColor = null,
                    UndefinedColor = (Color)123
                },
                formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.EnumComplexType, content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void NullableEnumParameter_Works_WithNotNullEnumValue()
        {
            // Arrange
            const string expect =
                "{\r\n" +
                "  \"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true\r\n" +
                "}";

            HttpConfiguration config = new[] { typeof(NullableEnumValueController) }.GetHttpConfiguration();
            config.MapODataServiceRoute("odata", "odata", GetSampleModel());
            HttpClient client = new HttpClient(new HttpServer(config));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/NullableEnumFunction(ColorParameter=System.Web.OData.Builder.TestModels.Color'Red')");

            // Act
            HttpResponseMessage respone = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(expect, respone.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void NullableEnumParameter_Works_WithNullEnumValue()
        {
            // Arrange
            const string expect =
                "{\r\n" +
                "  \"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":false\r\n" +
                "}";

            HttpConfiguration config = new[] { typeof(NullableEnumValueController) }.GetHttpConfiguration();
            config.MapODataServiceRoute("odata", "odata", GetSampleModel());
            HttpClient client = new HttpClient(new HttpServer(config));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/NullableEnumFunction(ColorParameter=null)");

            // Act
            HttpResponseMessage respone = client.SendAsync(request).Result;

            // Assert
            Assert.Equal(expect, respone.Content.ReadAsStringAsync().Result);
        }

        private static ODataMediaTypeFormatter GetFormatter()
        {
            var formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property })
            {
                Request = GetSampleRequest()
            };
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
            return formatter;
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            request.ODataProperties().Model = GetSampleModel();
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            request.SetFakeODataRouteName();
            return request;
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<EnumComplex>();

            FunctionConfiguration function = builder.Function("NullableEnumFunction").Returns<bool>();
            function.Parameter<Color?>("ColorParameter");

            return builder.GetEdmModel();
        }

        private class EnumComplex
        {
            public Color RequiredColor { get; set; }
            public Color? NullableColor { get; set; }
            public Color UndefinedColor { get; set; }
        }
    }

    public class NullableEnumValueController : ODataController
    {
        [HttpGet]
        [ODataRoute("NullableEnumFunction(ColorParameter={colorParameter})")]
        [EnableQuery]
        public bool NullableEnumFunction([FromODataUri]Color? colorParameter)
        {
            if (colorParameter != null)
            {
                return true;
            }

            Assert.True(ModelState.IsValid);
            return false;
        }
    }
}
