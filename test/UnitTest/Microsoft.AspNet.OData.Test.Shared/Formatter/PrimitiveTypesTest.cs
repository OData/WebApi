//-----------------------------------------------------------------------------
// <copyright file="PrimitiveTypesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class PrimitiveTypesTest
    {
        public static TheoryDataSet<Type, object, string, string> PrimitiveTypesToTest
        {
            get
            {
                string fullMetadata = ODataMediaTypes.ApplicationJsonODataFullMetadata;
                string noMetadata = ODataMediaTypes.ApplicationJsonODataNoMetadata;

                return new TheoryDataSet<Type, object, string, string>
                {
                    {typeof(string), "This is a Test String", fullMetadata, "StringFullMetadata.json"},
                    {typeof(string), "This is a Test String", noMetadata, "StringNoMetadata.json"},
                    {typeof(bool), true, fullMetadata, "BooleanFullMetadata.json"},
                    {typeof(byte), (byte)64, fullMetadata, "ByteFullMetadata.json"},
                    {typeof(byte[]), new byte[] { 0, 2, 32, 64, 128, 255 }, fullMetadata, "ArrayOfByteFullMetadata.json"},
                    {typeof(DateTimeOffset), new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), fullMetadata, "DateTimeOffsetFullMetadata.json"},
                    {typeof(Date), new Date(2014, 10, 14), fullMetadata, "DateFullMetadata.json"},
                    {typeof(TimeOfDay), new TimeOfDay(12, 13, 14, 15), fullMetadata, "TimeOfDayFullMetadata.json"},
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
                };
            }
        }

        [Theory]
        [MemberData(nameof(PrimitiveTypesToTest))]
        public async Task PrimitiveTypesSerializeAsOData(Type valueType, object value, string mediaType, string resourceName)
        {
            // Arrange
            string expectedEntity = Resources.GetString(resourceName);
            Assert.NotNull(expectedEntity); // Guard

            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            IEdmProperty property = model.EntityContainer.EntitySets().Single().EntityType().Properties().First();
            ODataPath path = new ODataPath(new PropertySegment(property as IEdmStructuralProperty));

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/WorkItems(10)/ID", configuration, "OData", path);

            var formatter = FormatterTestHelper.GetFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }, request, mediaType);

            // Act
            Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);
            var content = FormatterTestHelper.GetContent(value, type, formatter, mediaType.ToString());
            string actualEntity = await FormatterTestHelper.GetContentResult(content, request);

            // Assert
            Assert.NotNull(valueType);
            JsonAssert.Equal(expectedEntity, actualEntity);
        }

        [Theory]
        [MemberData(nameof(PrimitiveTypesToTest))]
        public async Task PrimitiveTypesDeserializeAsOData(Type valueType, object value, string mediaType, string resourceName)
        {
            // Arrange
            string entity = Resources.GetString(resourceName);
            Assert.NotNull(entity); // Guard

            object expectedValue = value;

            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData", c => c.AddService(ServiceLifetime.Singleton, b => model));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/WorkItems(10)/ID", configuration, "OData");

            var formatter = FormatterTestHelper.GetInputFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }, request, mediaType);

            // Act
            object actualValue = await FormatterTestHelper.ReadAsync(formatter, entity, valueType, request, mediaType);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        public static TheoryDataSet<Type, object, string, string> NullPrimitiveValueToTest
        {
            get
            {
                string fullMetadata = ODataMediaTypes.ApplicationJsonODataFullMetadata;
                string noMetadata = ODataMediaTypes.ApplicationJsonODataNoMetadata;

                return new TheoryDataSet<Type, object, string, string>
                {
                    // TODO: please remove the *.json file after ODL fixes the @odata.null issue.
                    {typeof(int?), (int?)null, fullMetadata, "NullableInt32FullMetadata.json"},
                    {typeof(int?), (int?)null, noMetadata, "NullableInt32NoMetadata.json"}
                };
            }
        }

        [Theory]
        [MemberData(nameof(NullPrimitiveValueToTest))]
        public async Task NullPrimitiveValueSerializeAsODataThrows(Type valueType, object value, string mediaType, string unused)
        {
            // Arrange
            Assert.NotNull(valueType);
            Assert.NotNull(unused);

            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();
            IEdmProperty property = model.EntityContainer.EntitySets().Single().EntityType().Properties().First();
            ODataPath path = new ODataPath(new PropertySegment(property as IEdmStructuralProperty));
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/WorkItems(10)/ID", configuration, "OData", path);

            var formatter = FormatterTestHelper.GetFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }, request, mediaType);

            // Act & Assert
            Type type = (value != null) ? value.GetType() : typeof(Nullable<int>);
            var content = FormatterTestHelper.GetContent(value, type, formatter, mediaType.ToString());
            await ExceptionAssert.ThrowsAsync<ODataException>(
                () => FormatterTestHelper.GetContentResult(content, request),
                "Cannot write the value 'null' in top level property; return 204 instead.");
        }

        [Theory]
        [MemberData(nameof(NullPrimitiveValueToTest))]
        public async Task NullPrimitiveValueDeserializeAsOData(Type valueType, object value, string mediaType, string resourceName)
        {
            // Arrange
            string entity = Resources.GetString(resourceName);
            Assert.NotNull(entity);

            object expectedValue = value;

            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData", c => c.AddService(ServiceLifetime.Singleton, b => model));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/WorkItems(10)/ID", configuration, "OData");

            var formatter = FormatterTestHelper.GetInputFormatter(new ODataPayloadKind[] { ODataPayloadKind.Property }, request, mediaType);

            // Act
            object actualValue = await FormatterTestHelper.ReadAsync(formatter, entity, valueType, request, mediaType);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
