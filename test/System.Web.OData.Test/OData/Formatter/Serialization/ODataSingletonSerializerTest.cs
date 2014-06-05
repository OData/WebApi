// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataSingletonSerializerTest
    {
        [Fact]
        public void CanSerializerSingleton()
        {
            // Arrange
            const string expect = "{" +
                "\"@odata.context\":\"http://localhost/odata/$metadata#Boss\"," +
                "\"EmployeeId\":987,\"EmployeeName\":\"John Mountain\"}";

            IEdmModel model = GetEdmModel();
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Boss");
            HttpRequestMessage request = GetRequest(model, singleton);
            ODataSerializerContext readContext = new ODataSerializerContext()
            {
                Url = new UrlHelper(request),
                Path = request.ODataProperties().Path,
                Model = model,
                NavigationSource = singleton
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider();
            EmployeeModel boss = new EmployeeModel {EmployeeId = 987, EmployeeName = "John Mountain"};
            MemoryStream bufferedStream = new MemoryStream();

            // Act
            ODataEntityTypeSerializer serializer = new ODataEntityTypeSerializer(serializerProvider);
            serializer.WriteObject(boss, typeof(EmployeeModel), GetODataMessageWriter(model, bufferedStream), readContext);

            // Assert
            string result = Encoding.UTF8.GetString(bufferedStream.ToArray());
            Assert.Equal(expect, result);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<EmployeeModel>("Boss");
            return builder.GetEdmModel();
        }

        private HttpRequestMessage GetRequest(IEdmModel model, IEdmSingleton singleton)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapODataServiceRoute("odata", "odata", model);
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);
            request.ODataProperties().PathHandler = new DefaultODataPathHandler();
            request.ODataProperties().RouteName = "odata";
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = new ODataPath(new[] { new SingletonPathSegment(singleton) });
            request.RequestUri = new Uri("http://localhost/odata/Boss");
            return request;
        }

        private ODataMessageWriter GetODataMessageWriter(IEdmModel model, MemoryStream bufferedStream)
        {
            HttpContent content = new StringContent("");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
            {
                PayloadBaseUri = new Uri("http://localhost/odata"),
                Version = ODataVersion.V4,
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://localhost/odata") }
            };

            IODataResponseMessage responseMessage = new ODataMessageWrapper(bufferedStream, content.Headers);
            return new ODataMessageWriter(responseMessage, writerSettings, model);
        }

        private sealed class EmployeeModel
        {
            [Key]
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }
    }
}
