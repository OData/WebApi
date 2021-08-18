//-----------------------------------------------------------------------------
// <copyright file="OperationSegmentTemplateTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Template
{
    public class OperationSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_OperationSegment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new OperationSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void Ctor_InitializeParameterMappingsProperty_Function()
        {
            // Arrange
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmFunction function = new EdmFunction("NS", "Function",
                new EdmEntityTypeReference(returnType, isNullable: false));

            var parameters = new[]
            {
                new OperationSegmentParameter("Parameter1", "{param1}"),
                new OperationSegmentParameter("Parameter2", "{param2}")
            };

            // Act
            var template = new OperationSegmentTemplate(
                new OperationSegment(new[] { function }, parameters, null));

            // Assert
            Assert.Equal(2, template.ParameterMappings.Count);
        }

        [Fact]
        public void TryMatch_ReturnTrue_IfSameFunction()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmFunction function = new EdmFunction("NS", "Function", new EdmEntityTypeReference(returnType, isNullable: false));
            function.AddParameter("Parameter1", EdmCoreModel.Instance.GetInt32(isNullable: false));
            function.AddParameter("Parameter2", EdmCoreModel.Instance.GetInt32(isNullable: false));

            var parameterValues = new[]
            {
                new OperationSegmentParameter("Parameter1", new ConstantNode(1, "1")),
                new OperationSegmentParameter("Parameter2", new ConstantNode(2, "2"))
            };

            var parameterMappings = new[]
            {
                new OperationSegmentParameter("Parameter1", "{param1}"),
                new OperationSegmentParameter("Parameter2", "{param2}")
            };

            OperationSegment segment = new OperationSegment(new[] { function }, parameterValues, entitySet: null);
            OperationSegmentTemplate template = new OperationSegmentTemplate(
                new OperationSegment(new[] { function }, parameterMappings, entitySet: null));

            // Act
            Dictionary<string, object> values = new Dictionary<string, object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal(5, values.Count);
            Assert.Equal(1, values["param1"]);
            Assert.Equal(2, values["param2"]);

            Assert.Equal(1, (values[ODataParameterValue.ParameterValuePrefix + "param1"] as ODataParameterValue).Value);
            Assert.Equal(2, (values[ODataParameterValue.ParameterValuePrefix + "param2"] as ODataParameterValue).Value);

            Assert.Equal(2, values[ODataRouteConstants.KeyCount]);
        }
    }
}
