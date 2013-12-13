// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Routing
{
    public class FunctionPathSegmentTemplateTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            Assert.ThrowsArgumentNull(() => new FunctionPathSegmentTemplate(function: null), "function");
        }

        [Fact]
        public void TryMatch_ReturnTrue_IfSameFunction()
        {
            // Arrange
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport function = new EdmFunctionImport(
                container,
                "Function",
                new EdmFunction(
                    "NS",
                    "Function",
                    new EdmEntityTypeReference(returnType, isNullable: false)));

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

            FunctionPathSegment segment = new FunctionPathSegment(function, model: null, parameterValues: parameterValues);
            FunctionPathSegmentTemplate template = new FunctionPathSegmentTemplate(
                new FunctionPathSegment(function, model: null, parameterValues: parameterMappings));


            // Act
            Dictionary<string, object> values = new Dictionary<string,object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Equal(2, values.Count);
            Assert.Equal("1", values["param1"]);
            Assert.Equal("2", values["param2"]);
        }
    }
}
