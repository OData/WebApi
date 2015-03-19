// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class BoundFunctionPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            Assert.ThrowsArgumentNull(() => new BoundFunctionPathSegmentTemplate(function: null), "function");
        }

        [Fact]
        public void Ctor_InitializeParameterMappingsProperty_Function()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunction function = new EdmFunction("NS", "Function",
                    new EdmEntityTypeReference(returnType, isNullable: false));

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>()
            {
                { "Parameter1", "{param1}" },
                { "Parameter2", "{param2}" }
            };

            // Act
            var template = new BoundFunctionPathSegmentTemplate(
                new BoundFunctionPathSegment(function, model, parameterMappings));

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

            Dictionary<string, string> parameterValues = new Dictionary<string, string>()
            {
                { "Parameter1", "1" },
                { "Parameter2", "2" }
            };

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>()
            {
                { "Parameter1", "{param1}" },
                { "Parameter2", "{param2}" }
            };

            BoundFunctionPathSegment segment = new BoundFunctionPathSegment(function, model, parameterValues: parameterValues);
            BoundFunctionPathSegmentTemplate template = new BoundFunctionPathSegmentTemplate(
                new BoundFunctionPathSegment(function, model, parameterValues: parameterMappings));

            // Act
            Dictionary<string, object> values = new Dictionary<string,object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal(4, values.Count);
            Assert.Equal("1", values["param1"]);
            Assert.Equal("2", values["param2"]);

            Assert.Equal(1, (values[ODataParameterValue.ParameterValuePrefix + "param1"] as ODataParameterValue).Value);
            Assert.Equal(2, (values[ODataParameterValue.ParameterValuePrefix + "param2"] as ODataParameterValue).Value);
        }
    }
}
