// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
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
        public static TheoryDataSet<string, Type, object> PrimitiveTypesToTest
        {
            get
            {
                return new TheoryDataSet<string, Type, object>
                {
                    {"StringInXml", typeof(string), "This is a Test String"},
                    {"BooleanInXml",typeof(bool), true},
                    {"ByteInXml",typeof(byte), (byte)64},
                    {"ArrayOfByteInXml",typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }},
                    {"DateTimeInXml", typeof(DateTime), new DateTime(2010, 1, 1)},
                    {"DecimalInXml",typeof(decimal), 12345.99999M},
                    {"DoubleInXml",typeof(double), 99999.12345},
                    {"GuidInXml", typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d")},
                    {"Int16InXml", typeof(short), Int16.MinValue},
                    {"Int32InXml",typeof(int), Int32.MinValue},
                    {"Int64InXml", typeof(long), Int64.MinValue},
                    {"SByteInXml",typeof(sbyte), SByte.MinValue},
                    {"SingleInXml",typeof(Single), Single.PositiveInfinity},
                    {"TimeSpanInXml", typeof(TimeSpan), TimeSpan.FromMinutes(60)},
                    {"NullableBooleanInXml",typeof(bool?), (bool?)false},
                    {"NullableInt32InXml",typeof(int?), (int?)null},
                };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesSerializeAsOData(string typeString, Type valueType, object value)
        {
            string expected = BaselineResource.ResourceManager.GetString(typeString);
            Assert.NotNull(expected);

            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.AddFakeODataRoute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties["MS_ODataPath"] = new DefaultODataPathHandler(model).Parse("WorkItems(10)/ID");

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request);

            Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);

            ObjectContent content = new ObjectContent(type, value, formatter);

            var result = content.ReadAsStringAsync().Result;
            Assert.Xml.Equal(expected, result);
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesDeserializeAsOData(string typeString, Type type, object value)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            configuration.AddFakeODataRoute();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request);
            ObjectContent content = new ObjectContent(type, value, formatter, "application/xml");

            var stream = content.ReadAsStreamAsync().Result;
            Assert.Equal(
                value,
                formatter.ReadFromStreamAsync(type, stream, content, new Mock<IFormatterLogger>().Object).Result);
        }

        private ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request)
        {
            return new ODataMediaTypeFormatter(model, new ODataPayloadKind[] { ODataPayloadKind.Property }, request);
        }
    }
}
