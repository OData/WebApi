// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
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
                    {"String", typeof(string), "This is a Test String"},
                    {"Bool",typeof(bool), true},
                    {"Byte",typeof(byte), (byte)64},
                    {"ByteArray",typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }},
                    {"DateTime", typeof(DateTime), new DateTime(2010, 1, 1)},
                    {"Decimal",typeof(decimal), 12345.99999M},
                    {"Double",typeof(double), 99999.12345},
                    {"Guid", typeof(Guid), new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d")},
                    {"Int16", typeof(short), Int16.MinValue},
                    {"Int32",typeof(int), Int32.MinValue},
                    {"Int64", typeof(long), Int64.MinValue},
                    {"SByte",typeof(sbyte), SByte.MinValue},
                    {"Single",typeof(Single), Single.PositiveInfinity},
                    {"TimeSpan", typeof(TimeSpan), TimeSpan.FromMinutes(60)},
                    {"NullableBool",typeof(bool?), (bool?)false},
                    {"NullableInt",typeof(int?), (int?)null},
                };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesSerializeAsOData(string typeString, Type valueType, object value)
        {
            string expected = BaselineResource.ResourceManager.GetString(typeString);
            Assert.NotNull(expected);

            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<WorkItem>("WorkItems");

            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            ODataMediaTypeFormatter formatter = CreateFormatter(model.GetEdmModel(), request);

            Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);

            ObjectContent content = new ObjectContent(type, value, formatter);

            var result = content.ReadAsStringAsync().Result;
            Assert.Xml.Equal(result, expected);
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        public void PrimitiveTypesDeserializeAsOData(string typeString, Type type, object value)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<WorkItem>("WorkItems");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            ODataMediaTypeFormatter formatter = CreateFormatter(model.GetEdmModel(), request);
            ObjectContent content = new ObjectContent(type, value, formatter);

            var stream = content.ReadAsStreamAsync().Result;
            Assert.Equal(
                value,
                formatter.ReadFromStreamAsync(type, stream, content, new Mock<IFormatterLogger>().Object).Result);
        }

        private ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request)
        {
            return new ODataMediaTypeFormatter(model, request);
        }
    }
}
