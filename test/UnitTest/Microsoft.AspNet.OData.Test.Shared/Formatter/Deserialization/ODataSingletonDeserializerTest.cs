//-----------------------------------------------------------------------------
// <copyright file="ODataSingletonDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class ODataSingletonDeserializerTest
    {
        private IEdmModel _edmModel;
        private IEdmSingleton _singleton;
        private readonly ODataDeserializerContext _readContext;
        private readonly ODataDeserializerProvider _deserializerProvider;

        private sealed class EmployeeModel
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }

        public ODataSingletonDeserializerTest()
        {
            EdmModel model = new EdmModel();
            var employeeType = new EdmEntityType("NS", "Employee");
            employeeType.AddStructuralProperty("EmployeeId", EdmPrimitiveTypeKind.Int32);
            employeeType.AddStructuralProperty("EmployeeName", EdmPrimitiveTypeKind.String);
            model.AddElement(employeeType);

            EdmEntityContainer defaultContainer = new EdmEntityContainer("NS", "Default");
            model.AddElement(defaultContainer);

            _singleton = new EdmSingleton(defaultContainer, "CEO", employeeType);
            defaultContainer.AddElement(_singleton);

            model.SetAnnotationValue<ClrTypeAnnotation>(employeeType, new ClrTypeAnnotation(typeof(EmployeeModel)));

            _edmModel = model;

            _readContext = new ODataDeserializerContext
            {
                Path = new ODataPath(new SingletonSegment(_singleton)),
                Model = _edmModel,
                ResourceType = typeof(EmployeeModel)
            };

            _deserializerProvider = ODataDeserializerProviderFactory.Create();
        }

        [Fact]
        public void CanDeserializerSingletonPayloadFromStream()
        {
            // Arrange
            const string payload = "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#CEO\"," +
                "\"EmployeeId\":789," +
                "\"EmployeeName\":\"John Hark\"}";

            ODataResourceDeserializer deserializer = new ODataResourceDeserializer(_deserializerProvider);

            // Act
            EmployeeModel employee = deserializer.Read(
                GetODataMessageReader(payload),
                typeof(EmployeeModel),
                _readContext) as EmployeeModel;

            // Assert
            Assert.NotNull(employee);
            Assert.Equal(789, employee.EmployeeId);
            Assert.Equal("John Hark", employee.EmployeeName);
        }

        private ODataMessageReader GetODataMessageReader(string content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/CEO");

            request.Content = new StringContent(content);
            request.Headers.Add("OData-Version", "4.0");

            MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            request.Headers.Accept.Add(mediaType);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new ODataMessageReader(new HttpRequestODataMessage(request), new ODataMessageReaderSettings(), _edmModel);
        }
    }
}
