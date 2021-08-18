//-----------------------------------------------------------------------------
// <copyright file="ODataSingletonSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
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
            var request = GetRequest(model, singleton);
            ODataSerializerContext readContext = new ODataSerializerContext()
            {
#if NETFX // Url is only in AspNet
                Url = new UrlHelper(request),
#endif
                Path = request.ODataContext().Path,
                Model = model,
                NavigationSource = singleton
            };

            ODataSerializerProvider serializerProvider = ODataSerializerProviderFactory.Create();
            EmployeeModel boss = new EmployeeModel {EmployeeId = 987, EmployeeName = "John Mountain"};
            MemoryStream bufferedStream = new MemoryStream();

            // Act
            ODataResourceSerializer serializer = new ODataResourceSerializer(serializerProvider);
            serializer.WriteObject(boss, typeof(EmployeeModel), GetODataMessageWriter(model, bufferedStream), readContext);

            // Assert
            string result = Encoding.UTF8.GetString(bufferedStream.ToArray());
            Assert.Equal(expect, result);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<EmployeeModel>("Boss");
            return builder.GetEdmModel();
        }

#if NETCORE
        private HttpRequest GetRequest(IEdmModel model, IEdmSingleton singleton)
#else
        private HttpRequestMessage GetRequest(IEdmModel model, IEdmSingleton singleton)
#endif
        {
            var config = RoutingConfigurationFactory.Create();
            config.MapODataServiceRoute("odata", "odata", model);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/odata/Boss", config, "odata");
            request.ODataContext().Path = new ODataPath(new[] { new SingletonSegment(singleton) });
            return request;
        }

        private ODataMessageWriter GetODataMessageWriter(IEdmModel model, MemoryStream bufferedStream)
        {
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings()
            {
                BaseUri = new Uri("http://localhost/odata"),
                Version = ODataVersion.V4,
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://localhost/odata") }
            };

            var headers = FormatterTestHelper.GetContentHeaders("application/json");
            IODataResponseMessage responseMessage = ODataMessageWrapperHelper.Create(bufferedStream, headers);
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
