// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter
{
    public class PrimitiveTypesTest
    {
        public static TheoryDataSet<Type, object, MediaTypeHeaderValue, string> PrimitiveTypesToTest
        {
            get
            {
                MediaTypeHeaderValue fullMetadata = ODataMediaTypes.ApplicationJsonODataFullMetadata;
                MediaTypeHeaderValue noMetadata = ODataMediaTypes.ApplicationJsonODataNoMetadata;
                MediaTypeHeaderValue xml = ODataMediaTypes.ApplicationXml;

                return new TheoryDataSet<Type, object, MediaTypeHeaderValue, string>
                {
                    {typeof(string), "This is a Test String", fullMetadata, "StringFullMetadata.json"},
                    {typeof(string), "This is a Test String", noMetadata, "StringNoMetadata.json"},
                    {typeof(bool), true, fullMetadata, "BooleanFullMetadata.json"},
                    {typeof(byte), (byte)64, fullMetadata, "ByteFullMetadata.json"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, fullMetadata, "ArrayOfByteFullMetadata.json"},
                    // TODO: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    //{typeof(DateTime), new DateTime(2010, 1, 1), fullMetadata, "DateTimeFullMetadata.json"},
                    {typeof(DateTimeOffset), new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), fullMetadata, "DateTimeOffsetFullMetadata.json"},
                    {typeof(decimal), 12345.99999M, fullMetadata, "DecimalFullMetadata.json"},
                    {typeof(double), 99999.12345, fullMetadata, "DoubleFullMetadata.json"},
                    {typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), fullMetadata, "GuidFullMetadata.json"},
                    {typeof(short), Int16.MinValue, fullMetadata, "Int16FullMetadata.json"},
                    {typeof(int), Int32.MinValue, fullMetadata, "Int32FullMetadata.json"},
                    {typeof(long), Int64.MinValue, fullMetadata, "Int64FullMetadata.json"},
                    {typeof(sbyte), SByte.MinValue, fullMetadata, "SByteFullMetadata.json"},
                    {typeof(Single), Single.PositiveInfinity, fullMetadata, "SingleFullMetadata.json"},
                    {typeof(TimeSpan), TimeSpan.FromMinutes(60), fullMetadata, "TimeSpanFullMetadata.json"},
                    {typeof(bool?), (bool?)false, fullMetadata, "NullableBooleanFullMetadata.json"},
                    {typeof(int?), (int?)null, fullMetadata, "NullableInt32FullMetadata.json"},
                    {typeof(int?), (int?)null, noMetadata, "NullableInt32NoMetadata.json"},
                };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesSerializeAsOData(Type valueType, object value, MediaTypeHeaderValue mediaType,
            string resourceName)
        {
            string expectedEntity = Resources.GetString(resourceName);
            Assert.NotNull(expectedEntity);

            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            string actualEntity;

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/WorkItems(10)/ID"))
            {
                request.SetConfiguration(configuration);
                IEdmProperty property =
                    model.EntityContainer.EntitySets().Single().EntityType().Properties().First();
                request.ODataProperties().Model = model;
                request.ODataProperties().Path = new ODataPath(new PropertyAccessPathSegment(property));
                request.SetFakeODataRouteName();

                ODataMediaTypeFormatter formatter = CreateFormatter(request);
                formatter.SupportedMediaTypes.Add(mediaType);

                Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);

                using (ObjectContent content = new ObjectContent(type, value, formatter))
                {
                    actualEntity = content.ReadAsStringAsync().Result;
                }
            }

            bool isJson = resourceName.EndsWith(".json");

            if (isJson)
            {
                JsonAssert.Equal(expectedEntity, actualEntity);
            }
            else
            {
                Assert.Xml.Equal(expectedEntity, actualEntity);
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesDeserializeAsOData(Type valueType, object value, MediaTypeHeaderValue mediaType,
            string resourceName)
        {
            string entity = Resources.GetString(resourceName);
            Assert.NotNull(entity);

            object expectedValue = value;

            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            object actualValue;

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID"))
            {
                HttpConfiguration config = new HttpConfiguration();
                config.MapODataServiceRoute("default", "", model);
                request.SetConfiguration(config);
                request.ODataProperties().RouteName = "default";
                request.ODataProperties().Model = model;

                ODataMediaTypeFormatter formatter = CreateFormatter(request);
                formatter.SupportedMediaTypes.Add(mediaType);

                using (StringContent content = new StringContent(entity))
                {
                    content.Headers.ContentType = mediaType;

                    using (Stream stream = content.ReadAsStreamAsync().Result)
                    {
                        actualValue = formatter.ReadFromStreamAsync(valueType, stream, content,
                            new Mock<IFormatterLogger>().Object).Result;
                    }
                }
            }

            Assert.Equal(expectedValue, actualValue);
        }

        private static HttpConfiguration CreateConfiguration()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Routes.MapFakeODataRoute();
            return configuration;
        }

        private ODataMediaTypeFormatter CreateFormatter(HttpRequestMessage request)
        {
            return new ODataMediaTypeFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }) { Request = request };
        }
    }
}
