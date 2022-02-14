//-----------------------------------------------------------------------------
// <copyright file="EnumSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter.Deserialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#else
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.AspNet.OData.Test.Formatter.Deserialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class EnumSerializerTest
    {
        private readonly ODataSerializerProvider _serializerProvider =
            ODataSerializerProviderFactory.Create();

        [Fact]
        public void GetEdmTypeSerializer_ReturnODataEnumSerializer_ForEnumType()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEnumTypeReference(new EdmEnumType("TestModel", "Color"), isNullable: false);

            // Act
            ODataEdmTypeSerializer serializer = _serializerProvider.GetEdmTypeSerializer(edmType);

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
            ExceptionAssert.ThrowsArgumentNull(
                () =>
                    new ODataEnumSerializer(_serializerProvider).WriteObject(graph, type, messageWriter,
                        writeContext),
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
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataEnumSerializer(_serializerProvider).WriteObject(graph, type, messageWriter, writeContext),
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
            ExceptionAssert.ThrowsArgument(
                () => new ODataEnumSerializer(_serializerProvider).WriteObject(graph, type, messageWriter, writeContext),
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
            ExceptionAssert.Throws<InvalidOperationException>(
                () => new ODataEnumSerializer(_serializerProvider).CreateODataValue(graph, expectedType, writeContext),
                "ODataEnumSerializer cannot write an object of type 'Edm.Int32'.");
        }

#if !NETCORE // TODO #939: Enable this test on AspNetCore.
        [Fact]
        public async Task EnumTypeSerializerTestForOData()
        {
            // Arrange
            string enumComplexPayload = @"{
  ""@odata.context"":""http://localhost/$metadata#Microsoft.AspNet.OData.Test.EnumComplex"",""RequiredColor"":""Red, Blue"",""NullableColor"":null,""UndefinedColor"":""123""
}";

            ODataMediaTypeFormatter formatter = GetFormatter();
            ObjectContent<EnumComplex> content = new ObjectContent<EnumComplex>(
                new EnumComplex()
                {
                    RequiredColor = Color.Red | Color.Blue,
                    NullableColor = null,
                    UndefinedColor = (Color)123
                },
                formatter,
                MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));

            // Act & Assert
            JsonAssert.Equal(enumComplexPayload, await content.ReadAsStringAsync());
        }
#endif
        [Fact]
        public async Task NullableEnumParameter_Works_WithNotNullEnumValue()
        {
            // Arrange
            const string expect =
                "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true" +
                "}";

            var controllers = new[] { typeof(NullableEnumValueController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetSampleModel());
            });
            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/NullableEnumFunction(ColorParameter=Microsoft.AspNet.OData.Test.Builder.TestModels.Color'Red')");

            // Act
            HttpResponseMessage respone = await client.SendAsync(request);

            // Assert
            Assert.Equal(expect, await respone.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task NullableEnumParameter_Works_WithNullEnumValue()
        {
            // Arrange
            const string expect =
                "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":false" +
                "}";

            var controllers = new[] { typeof(NullableEnumValueController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetSampleModel());
            });
            HttpClient client = TestServerFactory.CreateClient(server);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/NullableEnumFunction(ColorParameter=null)");

            // Act
            HttpResponseMessage respone = await client.SendAsync(request);

            // Assert
            Assert.Equal(expect, await respone.Content.ReadAsStringAsync());
        }

#if !NETCORE
        private static ODataMediaTypeFormatter GetFormatter()
        {
            var formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Resource })
            {
                Request = GetSampleRequest()
            };
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationXml));
            return formatter;
        }

        private static HttpRequestMessage GetSampleRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/property");
            request.EnableODataDependencyInjectionSupport(GetSampleModel());
            request.GetConfiguration().Routes.MapFakeODataRoute();
            return request;
        }
#endif
        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
