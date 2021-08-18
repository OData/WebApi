//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeTest.cs" company=".NET Foundation">
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
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        [Fact]
        public async Task ComplexTypeSerializesAsOData()
        {
            // Arrange
            string routeName = "OData";
            IEdmModel model = GetSampleModel();
            var config = RoutingConfigurationFactory.Create();
            config = RoutingConfigurationFactory.CreateWithRootContainer(routeName, b => b.AddService(ServiceLifetime.Singleton, s => model));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/property", config, routeName);
            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = FormatterTestHelper.GetFormatter(payload, request);
            var content = FormatterTestHelper.GetContent(new Person(0, new ReferenceDepthContext(7)), formatter,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);

            // Act & Assert
            JsonAssert.Equal(Resources.PersonComplexType, await FormatterTestHelper.GetContentResult(content, request));
        }

        private static IEdmModel GetSampleModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<Person>();

            // Employee is derived from Person. Employee has a property named manager it's Employee type.
            // It's not allowed to build inheritance complex type because a recursive loop of complex types is not allowed.
            builder.Ignore<Employee>();
            return builder.GetEdmModel();
        }
    }
}
