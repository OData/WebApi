// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Common.Models;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.AspNet.OData.Formatter.Serialization
{
    public class EntityTypeTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public async Task EntityTypeSerializesAsODataEntry()
        {
            // Arrange
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            var config = RoutingConfigurationFactory.CreateWithRootContainer("Route");
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/property", config);
            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = FormatterTestHelper.GetFormatter(payload, request, _model, "Route", path);
            Employee employee = (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee);
            var content = FormatterTestHelper.GetContent(employee, formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.EmployeeEntry, await FormatterTestHelper.GetContentResult(content, request));
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Employee>("employees");
            builder.EntitySet<WorkItem>("workitems");
            return builder.GetEdmModel();
        }
    }
}
