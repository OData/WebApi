// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class PrimitiveTypesTest
    {
        public static TheoryDataSet<Type, object, MediaTypeHeaderValue, string> PrimitiveTypesToTest
        {
            get
            {
                MediaTypeHeaderValue minimalMetadata = ODataMediaTypes.ApplicationJsonODataMinimalMetadata;
                MediaTypeHeaderValue noMetadata = ODataMediaTypes.ApplicationJsonODataNoMetadata;
                MediaTypeHeaderValue xml = ODataMediaTypes.ApplicationXml;

                return new TheoryDataSet<Type, object, MediaTypeHeaderValue, string>
                {
                    {typeof(string), "This is a Test String", minimalMetadata, "StringInJsonMinimalMetadata.json"},
                    {typeof(string), "This is a Test String", noMetadata, "StringInJsonNoMetadata.json"},
                    {typeof(string), "This is a Test String", xml, "StringInXml.xml"},
                    {typeof(bool), true, minimalMetadata, "BooleanInJsonMinimalMetadata.json"},
                    {typeof(bool), true, xml, "BooleanInXml.xml"},
                    {typeof(byte), (byte)64, minimalMetadata, "ByteInJsonMinimalMetadata.json"},
                    {typeof(byte), (byte)64, xml, "ByteInXml.xml"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, minimalMetadata,
                        "ArrayOfByteInJsonMinimalMetadata.json"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, xml, "ArrayOfByteInXml.xml"},
                    {typeof(DateTime), new DateTime(2010, 1, 1), minimalMetadata,
                        "DateTimeInJsonMinimalMetadata.json"},
                    {typeof(DateTime), new DateTime(2010, 1, 1), xml, "DateTimeInXml.xml"},
                    {typeof(decimal), 12345.99999M, minimalMetadata, "DecimalInJsonMinimalMetadata.json"},
                    {typeof(decimal), 12345.99999M, xml, "DecimalInXml.xml"},
                    {typeof(double), 99999.12345, minimalMetadata, "DoubleInJsonMinimalMetadata.json"},
                    {typeof(double), 99999.12345, xml, "DoubleInXml.xml"},
                    {typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), minimalMetadata,
                        "GuidInJsonMinimalMetadata.json"},
                    {typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), xml, "GuidInXml.xml"},
                    {typeof(short), Int16.MinValue, minimalMetadata, "Int16InJsonMinimalMetadata.json"},
                    {typeof(short), Int16.MinValue, xml, "Int16InXml.xml"},
                    {typeof(int), Int32.MinValue, minimalMetadata, "Int32InJsonMinimalMetadata.json"},
                    {typeof(int), Int32.MinValue, xml, "Int32InXml.xml"},
                    {typeof(long), Int64.MinValue, minimalMetadata, "Int64InJsonMinimalMetadata.json"},
                    {typeof(long), Int64.MinValue, xml, "Int64InXml.xml"},
                    {typeof(sbyte), SByte.MinValue, minimalMetadata, "SByteInJsonMinimalMetadata.json"},
                    {typeof(sbyte), SByte.MinValue, xml, "SByteInXml.xml"},
                    {typeof(Single), Single.PositiveInfinity, minimalMetadata, "SingleInJsonMinimalMetadata.json"},
                    {typeof(Single), Single.PositiveInfinity, xml, "SingleInXml.xml"},
                    {typeof(TimeSpan), TimeSpan.FromMinutes(60), minimalMetadata, "TimeSpanInJsonMinimalMetadata.json"},
                    {typeof(TimeSpan), TimeSpan.FromMinutes(60), xml, "TimeSpanInXml.xml"},
                    {typeof(bool?), (bool?)false, minimalMetadata, "NullableBooleanInJsonMinimalMetadata.json"},
                    {typeof(bool?), (bool?)false, xml, "NullableBooleanInXml.xml"},
                    {typeof(int?), (int?)null, minimalMetadata, "NullableInt32InJsonMinimalMetadata.json"},
                    {typeof(int?), (int?)null, noMetadata, "NullableInt32InJsonNoMetadata.json"},
                    {typeof(int?), (int?)null, xml, "NullableInt32InXml.xml"},
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
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
                IEdmProperty property =
                    model.EntityContainers().Single().EntitySets().Single().ElementType.Properties().First();
                request.SetODataPath(new ODataPath(new PropertyAccessPathSegment(property)));

                ODataMediaTypeFormatter formatter = CreateFormatter(model, request);
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

            using (HttpConfiguration configuration = CreateConfiguration())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/WorkItems(10)/ID"))
            {
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

                ODataMediaTypeFormatter formatter = CreateFormatter(model, request);
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
            configuration.AddFakeODataRoute();
            return configuration;
        }

        private ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request)
        {
            return new ODataMediaTypeFormatter(model, new ODataPayloadKind[] { ODataPayloadKind.Property }, request);
        }
    }
}
