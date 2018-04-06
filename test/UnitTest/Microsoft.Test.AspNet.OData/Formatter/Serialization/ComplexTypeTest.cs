// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Common;
using Microsoft.Test.AspNet.OData.Common.Models;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Formatter.Serialization
{
    public class ComplexTypeTest
    {
        [Fact]
        public async Task ComplexTypeSerializesAsOData()
        {
            // Arrange
            IEdmModel model = GetSampleModel();
            var config = RoutingConfigurationFactory.Create();
            config = RoutingConfigurationFactory.CreateWithRootContainer("OData", b => b.AddService(ServiceLifetime.Singleton, s => model));
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/property", config);
            var payload = new ODataPayloadKind[] { ODataPayloadKind.Resource };
            var formatter = FormatterTestHelper.GetFormatter(payload, request, model);
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
