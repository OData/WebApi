//-----------------------------------------------------------------------------
// <copyright file="EntityTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class EntityTypeTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public async Task EntityTypeSerializesAsODataEntry()
        {
            // Arrange
            const string routeName = "OData";
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            var config = RoutingConfigurationFactory.CreateWithRootContainer(routeName, b => b.AddService(ServiceLifetime.Singleton, s => _model));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/property", config, routeName, path);
            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = FormatterTestHelper.GetFormatter(payload, request);
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
