// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class UnboundFunctionPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_UnboundFunction()
        {
            Assert.ThrowsArgumentNull(() => new UnboundFunctionPathSegmentTemplate(function: null), "function");
        }

        [Fact]
        public void Ctor_InitializeParameterMappingsProperty_UnboundFunction()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport function = new EdmFunctionImport(
                container,
                "Function",
                new EdmFunction(
                    "NS",
                    "Function",
                    new EdmEntityTypeReference(returnType, isNullable: false)));

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "Parameter1", "{param1}" },
                { "Parameter2", "{param2}" }
            };

            // Act
            var template = new UnboundFunctionPathSegmentTemplate(
                new UnboundFunctionPathSegment(function, model, parameterMappings));

            // Assert
            Assert.Equal(2, template.ParameterMappings.Count);
        }

        [Fact]
        public void TryMatch_ReturnTrue_IfSameUnboundFunction()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport function = new EdmFunctionImport(
                container,
                "Function",
                new EdmFunction(
                    "NS",
                    "Function",
                    new EdmEntityTypeReference(returnType, isNullable: false)));

            Dictionary<string, string> parameterValues = new Dictionary<string, string>
            {
                { "Parameter1", "1" },
                { "Parameter2", "2" }
            };

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "Parameter1", "{param1}" },
                { "Parameter2", "{param2}" }
            };

            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(function, model, parameterValues);
            UnboundFunctionPathSegmentTemplate template = new UnboundFunctionPathSegmentTemplate(
                new UnboundFunctionPathSegment(function, model, parameterMappings));

            // Act
            Dictionary<string, object> values = new Dictionary<string,object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal(2, values.Count);
            Assert.Equal("1", values["param1"]);
            Assert.Equal("2", values["param2"]);
        }

        [Fact]
        public void TryMatch_ReturnsFalse_IfDifferentUnboundFunctionWithSameParamerters()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunction function = new EdmFunction("NS", "Function", new EdmEntityTypeReference(returnType, isNullable: false));
            EdmFunctionImport functionImport1 = new EdmFunctionImport(container,"FunctionImport1", function);
            EdmFunctionImport functionImport2 = new EdmFunctionImport(container,"FunctionImport2", function);

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

            UnboundFunctionPathSegment segment = new UnboundFunctionPathSegment(functionImport1, model, parameterValues);
            var template = new UnboundFunctionPathSegmentTemplate(
                new UnboundFunctionPathSegment(functionImport2, model, parameterMappings));
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.False(result);
        }
    }
}
