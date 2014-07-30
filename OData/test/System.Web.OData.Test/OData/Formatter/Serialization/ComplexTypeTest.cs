// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        private readonly ODataMediaTypeFormatter _formatter;

        public ComplexTypeTest()
        {
            _formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }) { Request = GetSampleRequest() };
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            _formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
        }

        [Fact]
        public void ComplexTypeSerializesAsOData()
        {
            // Arrange
            ObjectContent<Person> content = new ObjectContent<Person>(new Person(0, new ReferenceDepthContext(7)),
                _formatter, ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.PersonComplexType, content.ReadAsStringAsync().Result);
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
            builder.ComplexType<Person>();
            return builder.GetEdmModel();
        }
    }
}
