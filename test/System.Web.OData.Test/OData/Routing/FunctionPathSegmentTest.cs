// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Routing
{
    public class FunctionPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Function()
        {
            // Act
            IEdmModel model = new EdmModel();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Assert.ThrowsArgumentNull(() => new FunctionPathSegment(function: null, model: model, parameterValues: parameters),
                "function");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_FunctionName()
        {
            // Act
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Assert.ThrowsArgumentNull(() => new FunctionPathSegment(functionName: null, parameterValues: parameters),
                "functionName");
        }

        [Fact]
        public void Property_SegmentKind_IsEntitySet()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            FunctionPathSegment segment = new FunctionPathSegment("function", parameters);

            Assert.Equal(ODataSegmentKinds.Function, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_Returns_FunctionReturnType()
        {
            // Arrange
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport function = new EdmFunctionImport(
                container,
                "Function",
                new EdmFunction("NS", "Function", new EdmEntityTypeReference(returnType, isNullable: false)));
            FunctionPathSegment segment = new FunctionPathSegment(function, model: null, parameterValues: null);

            // Act
            var result = segment.GetEdmType(previousEdmType: null);

            // Assert
            Assert.Same(returnType, result);
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfSameFunction()
        {
            // Arrange
            IEdmEntityType returnType = new Mock<IEdmEntityType>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmFunctionImport function = new EdmFunctionImport(
                container,
                "Function",
                new EdmFunction("NS", "Function", new EdmEntityTypeReference(returnType, isNullable: false)));

            FunctionPathSegment template = new FunctionPathSegment(function, model: null, parameterValues: null);
            FunctionPathSegment segment = new FunctionPathSegment(function, model: null, parameterValues: null);

            // Act
            Dictionary<string, object> values = new Dictionary<string,object>();
            bool result = template.TryMatch(segment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }
    }
}
