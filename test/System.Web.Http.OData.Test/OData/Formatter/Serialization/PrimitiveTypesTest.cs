// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class PrimitiveTypesTest
    {
        public static TheoryDataSet<string, object> PrimitiveTypesToTest
        {
            get
            {
                return new TheoryDataSet<string, object>
                {
                    {"String", "This is a Test String"},
                    {"Bool", true},
                    {"Byte", (byte)64},
                    {"ByteArray", new byte[] { 0, 2, 32, 64, 128, 255 }},
                    {"DateTime", new DateTime(2010, 1, 1)},
                    {"Decimal", 12345.99999M},
                    {"Double", 99999.12345},
                    {"Guid", new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d")},
                    {"Int16", Int16.MinValue},
                    {"Int32", Int32.MinValue},
                    {"Int64", Int64.MinValue},
                    {"SByte", SByte.MinValue},
                    {"Single", Single.PositiveInfinity},
                    {"TimeSpan", TimeSpan.FromMinutes(60)},
                    {"NullableBool", (bool?)false},
                    {"NullableInt", (int?)null},
                };
            }
        }

        [Theory]
        [PropertyData("PrimitiveTypesToTest")]
        [Trait("Description", "ODataMediaTypeFormatter serializes primitive types in valid ODataMessageFormat")]
        public void PrimitiveTypesSerializeAsOData(string typeString, object value)
        {
            string expected = BaselineResource.ResourceManager.GetString(typeString);
            Assert.NotNull(expected);

            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<WorkItem>("WorkItems");

            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)/ID");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(model.GetEdmModel()) { Request = request };

            Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);

            ObjectContent content = new ObjectContent(type, value, formatter);

            var result = content.ReadAsStringAsync().Result;
            Assert.Xml.Equal(result, expected);
        }
    }
}
