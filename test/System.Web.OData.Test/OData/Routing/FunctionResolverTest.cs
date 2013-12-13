// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class FunctionResolverTest
    {
        [Theory]
        [InlineData("FunctionWithoutParams", "()", "NS.Name.FunctionWithoutParams()")] // function without params using explicit empty parameter list
        [InlineData("FunctionWithoutParams", null, "NS.Name.FunctionWithoutParams()")] // function without params concise invocation
        [InlineData("FunctionWithoutParams", "non parameter list", "NS.Name.FunctionWithoutParams()")] // function without params concise invocation
        [InlineData("FunctionWithOneParam", "(Parameter=1)", "NS.Name.FunctionWithOneParam(Parameter=1)")] // function with one param
        [InlineData("FunctionWithOneParam", "(Parameter=@1)", "NS.Name.FunctionWithOneParam(Parameter=@1)")] // function with one param and aliased value
        [InlineData("FunctionWithOneParam", "(Parameter='1')", "NS.Name.FunctionWithOneParam(Parameter='1')")] // function with one param string value
        [InlineData("FunctionWithMultipleParams", "(Parameter1=1,Parameter2=2,Parameter3=3)", "NS.Name.FunctionWithMultipleParams(Parameter1=1,Parameter2=2,Parameter3=3)")] // function with multiple params
        [InlineData("FunctionWithMultipleParams", "(Parameter2=1,Parameter3=2,Parameter1=3)", "NS.Name.FunctionWithMultipleParams(Parameter2=1,Parameter3=2,Parameter1=3)")] // function with multiple params, different order
        [InlineData("FunctionWithOverloads", "()", "NS.Name.FunctionWithOverloads()")] // overloaded function, empty parameter overload
        [InlineData("FunctionWithOverloads", null, "NS.Name.FunctionWithOverloads()")] // overloaded function, empty parameter overload, concise notation
        [InlineData("FunctionWithOverloads", "(Parameter=1)", "NS.Name.FunctionWithOverloads(Parameter=1)")] // overloaded function, one param
        [InlineData("FunctionWithOverloads", "(Parameter1=1,Parameter2=2,Parameter3=3)", "NS.Name.FunctionWithOverloads(Parameter1=1,Parameter2=2,Parameter3=3)")] // overloaded function, multiple params
        [InlineData("FunctionWithOverloads", "(Parameter2=2,Parameter1=1,Parameter3=3)", "NS.Name.FunctionWithOverloads(Parameter2=2,Parameter1=1,Parameter3=3)")] // overloaded function, multiple params - random order
        public void TryResolve(string functionName, string nextSegment, string expectedResult)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmFunctionImport> functions =
                container.OperationImports().Where(o => o.Name == functionName).OfType<IEdmFunctionImport>();

            // Act
            FunctionPathSegment pathSegment = FunctionResolver.TryResolve(functions, model, nextSegment);

            // Assert
            Assert.NotNull(pathSegment);
            Assert.Equal(expectedResult, pathSegment.ToString());
        }

        [Theory]
        [InlineData("FunctionWithOutParams", "(somekey=1)")] // empty function and index on the result should fail as the caller should invoke the function explicitly with empty params
        [InlineData("FunctionWithOneParam", null)] // no parameters
        [InlineData("FunctionWithOneParam", "()")] // empty parameters
        [InlineData("FunctionWithOneParam", "something")] // not a function call parameter list
        [InlineData("FunctionWithOneParam", "(UnknownParam=42)")] // unknown parameter
        [InlineData("FunctionWithOneParam", "(UnknownParam1=42,UnknownParam2=42)")] // unknown parameters
        [InlineData("FunctionWithOneParam", "(Parameter=42,UnknownParam2=42)")] // known and unknown parameters
        [InlineData("FunctionWithMultipleParams", "(Parameter1=42,Parameter2=42)")] // subset parameters
        public void TryResolve_NegativeTests(string functionName, string nextSegment)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmFunctionImport> functions =
                container.OperationImports().Where(o => o.Name == functionName).OfType<IEdmFunctionImport>();

            // Act & Assert
            Assert.Null(FunctionResolver.TryResolve(functions, model, nextSegment));
        }

        private IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Name");
            model.AddElement(container);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            container.AddFunctionImport(new EdmFunction("NS", "FunctionWithoutParams", returnType));

            var functionWithOneParam = new EdmFunction("NS", "FunctionWithOneParam", returnType);
            functionWithOneParam.AddParameter("Parameter", parameterType);
            container.AddFunctionImport(functionWithOneParam);

            var functionWithMultipleParams = new EdmFunction("NS", "FunctionWithMultipleParams", returnType);
            functionWithMultipleParams.AddParameter("Parameter1", parameterType);
            functionWithMultipleParams.AddParameter("Parameter2", parameterType);
            functionWithMultipleParams.AddParameter("Parameter3", parameterType);
            container.AddFunctionImport(functionWithMultipleParams);

            container.AddFunctionImport(new EdmFunction("NS", "FunctionWithOverloads", returnType));
            var functionWithOverloads2 = new EdmFunction("NS", "FunctionWithOverloads", returnType);
            functionWithOverloads2.AddParameter("Parameter", parameterType);
            container.AddFunctionImport(functionWithOverloads2);
            var functionWithOverloads3 = new EdmFunction("NS", "FunctionWithOverloads", returnType);
            functionWithOverloads3.AddParameter("Parameter1", parameterType);
            functionWithOverloads3.AddParameter("Parameter2", parameterType);
            functionWithOverloads3.AddParameter("Parameter3", parameterType);
            container.AddFunctionImport(functionWithOverloads3);

            return model;
        }
    }
}
