//-----------------------------------------------------------------------------
// <copyright file="ResourceSetTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ResourceSetTest
    {
        private IEdmModel _model = GetSampleModel();

        [Fact]
        public async Task IEnumerableOfEntityTypeSerializesAsODataResourceSet()
        {
            // Arrange
            IEdmEntitySet entitySet = _model.EntityContainer.FindEntitySet("employees");
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));

            var request = RequestFactory.CreateFromModel(_model, "http://localhost/property", "Route", path);
            var payload = new ODataPayloadKind[] { ODataPayloadKind.ResourceSet };
            var formatter = FormatterTestHelper.GetFormatter(payload, request);

            IEnumerable<Employee> collectionOfPerson = new Collection<Employee>()
            {
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 0),
                (Employee)TypeInitializer.GetInstance(SupportedTypes.Employee, 1),
            };

            var content = FormatterTestHelper.GetContent(collectionOfPerson, formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.FeedOfEmployee, await FormatterTestHelper.GetContentResult(content, request));
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
