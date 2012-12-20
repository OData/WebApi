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
                    {typeof(string), "This is a Test String", minimalMetadata, "StringInJsonMinimalMetadata"},
                    {typeof(string), "This is a Test String", noMetadata, "StringInJsonNoMetadata"},
                    {typeof(string), "This is a Test String", xml, "StringInXml"},
                    {typeof(bool), true, minimalMetadata, "BooleanInJsonMinimalMetadata"},
                    {typeof(bool), true, xml, "BooleanInXml"},
                    {typeof(byte), (byte)64, minimalMetadata, "ByteInJsonMinimalMetadata"},
                    {typeof(byte), (byte)64, xml, "ByteInXml"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, minimalMetadata,
                        "ArrayOfByteInJsonMinimalMetadata"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, xml, "ArrayOfByteInXml"},
                    {typeof(DateTime), new DateTime(2010, 1, 1), minimalMetadata, "DateTimeInJsonMinimalMetadata"},
                    {typeof(DateTime), new DateTime(2010, 1, 1), xml, "DateTimeInXml"},
                    {typeof(decimal), 12345.99999M, minimalMetadata, "DecimalInJsonMinimalMetadata"},
                    {typeof(decimal), 12345.99999M, xml, "DecimalInXml"},
                    {typeof(double), 99999.12345, minimalMetadata, "DoubleInJsonMinimalMetadata"},
                    {typeof(double), 99999.12345, xml, "DoubleInXml"},
                    {typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), minimalMetadata,
                        "GuidInJsonMinimalMetadata"},
                    {typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), xml, "GuidInXml"},
                    {typeof(short), Int16.MinValue, minimalMetadata, "Int16InJsonMinimalMetadata"},
                    {typeof(short), Int16.MinValue, xml, "Int16InXml"},
                    {typeof(int), Int32.MinValue, minimalMetadata, "Int32InJsonMinimalMetadata"},
                    {typeof(int), Int32.MinValue, xml, "Int32InXml"},
                    {typeof(long), Int64.MinValue, minimalMetadata, "Int64InJsonMinimalMetadata"},
                    {typeof(long), Int64.MinValue, xml, "Int64InXml"},
                    {typeof(sbyte), SByte.MinValue, minimalMetadata, "SByteInJsonMinimalMetadata"},
                    {typeof(sbyte), SByte.MinValue, xml, "SByteInXml"},
                    {typeof(Single), Single.PositiveInfinity, minimalMetadata, "SingleInJsonMinimalMetadata"},
                    {typeof(Single), Single.PositiveInfinity, xml, "SingleInXml"},
                    {typeof(TimeSpan), TimeSpan.FromMinutes(60), minimalMetadata, "TimeSpanInJsonMinimalMetadata"},
                    {typeof(TimeSpan), TimeSpan.FromMinutes(60), xml, "TimeSpanInXml"},
                    {typeof(bool?), (bool?)false, minimalMetadata, "NullableBooleanInJsonMinimalMetadata"},
                    {typeof(bool?), (bool?)false, xml, "NullableBooleanInXml"},
                    {typeof(int?), (int?)null, minimalMetadata, "NullableInt32InJsonMinimalMetadata"},
                    {typeof(int?), (int?)null, noMetadata, "NullableInt32InJsonNoMetadata"},
                    {typeof(int?), (int?)null, xml, "NullableInt32InXml"},
                };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesSerializeAsOData(Type valueType, object value, MediaTypeHeaderValue mediaType,
            string resourceName)
        {
            string expectedEntity = BaselineResource.ResourceManager.GetString(resourceName);
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

            Assert.Equal(expectedEntity, actualEntity);
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesDeserializeAsOData(Type valueType, object value, MediaTypeHeaderValue mediaType,
            string resourceName)
        {
            string entity = BaselineResource.ResourceManager.GetString(resourceName);
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
