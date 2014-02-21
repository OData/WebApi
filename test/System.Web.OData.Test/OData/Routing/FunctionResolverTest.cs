// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class FunctionResolverTest
    {
        [Theory]
        [InlineData("BoundFunctionWithoutParams", "()", "NS.BoundFunctionWithoutParams()")]
        [InlineData("BoundFunctionWithoutParams", null, "NS.BoundFunctionWithoutParams()")]
        [InlineData("BoundFunctionWithoutParams", "non parameter list", "NS.BoundFunctionWithoutParams()")]
        [InlineData("BoundFunctionWithOneParam", "(Parameter=1)", "NS.BoundFunctionWithOneParam(Parameter=1)")]
        [InlineData("BoundFunctionWithOneParam", "(Parameter=@1)", "NS.BoundFunctionWithOneParam(Parameter=@1)")]
        [InlineData("BoundFunctionWithOneParam", "(Parameter='1')", "NS.BoundFunctionWithOneParam(Parameter='1')")]
        [InlineData("BoundFunctionWithMultipleParams", "(Parameter1=1,Parameter2=2,Parameter3=3)", "NS.BoundFunctionWithMultipleParams(Parameter1=1,Parameter2=2,Parameter3=3)")] // function with multiple params
        [InlineData("BoundFunctionWithMultipleParams", "(Parameter2=1,Parameter3=2,Parameter1=3)", "NS.BoundFunctionWithMultipleParams(Parameter2=1,Parameter3=2,Parameter1=3)")] // function with multiple params, different order
        [InlineData("BoundFunctionOverload", "()", "NS.BoundFunctionOverload()")] // overloaded function, empty parameter overload
        [InlineData("BoundFunctionOverload", null, "NS.BoundFunctionOverload()")] 
        [InlineData("BoundFunctionOverload", "(Parameter=1)", "NS.BoundFunctionOverload(Parameter=1)")] // overloaded function, one param
        [InlineData("BoundFunctionOverload", "(Parameter1=1,Parameter2=2,Parameter3=3)", "NS.BoundFunctionOverload(Parameter1=1,Parameter2=2,Parameter3=3)")] // overloaded function, multiple params
        [InlineData("BoundFunctionOverload", "(Parameter2=2,Parameter1=1,Parameter3=3)", "NS.BoundFunctionOverload(Parameter2=2,Parameter1=1,Parameter3=3)")] // overloaded function, multiple params - different order
        public void TryResolveBound(string functionName, string nextSegment, string expectedResult)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEnumerable<IEdmFunction> functions = model.SchemaElements.OfType<IEdmFunction>().Where(f => f.Name == functionName);

            // Act
            BoundFunctionPathSegment pathSegment = FunctionResolver.TryResolveBound(functions, model, nextSegment);

            // Assert
            Assert.NotNull(pathSegment);
            Assert.Equal(expectedResult, pathSegment.ToString());
        }

        [Theory]
        [InlineData("BoundFunctionWithoutParams", "(somekey=1)")]
        [InlineData("BoundFunctionWithOneParam", null)]
        [InlineData("BoundFunctionWithOneParam", "()")]
        [InlineData("BoundFunctionWithOneParam", "something")]
        [InlineData("BoundFunctionWithOneParam", "(UnknownParam=1)")]
        [InlineData("BoundFunctionWithOneParam", "(UnknownParam1=1,UnknownParam2=2)")]
        [InlineData("BoundFunctionWithOneParam", "(Parameter=1,UnknownParam2=2)")]
        [InlineData("BoundFunctionWithMultipleParams", "(Parameter1=1,Parameter2=2)")]
        [InlineData("BoundFunctionWithMultipleParams", "(Parameter1=1,Parameter2=2,UnknownParam3=3)")]
        [InlineData("BoundFunctionWithMultipleParams", "(Parameter1=1,Parameter2=2,Parameter3=3,UnknownParam4=4)")]
        [InlineData("BoundFunctionOverload", "(UnknownParam1=1)")]
        [InlineData("BoundFunctionOverload", "(Parameter1=1,Parameter2=2,UnknownParam3=3)")]
        [InlineData("BoundFunctionOverload", "(Parameter1=1,Parameter2=2,Parameter3=3,UnknownParam4=4)")]
        public void TryResolveBound_NegativeTests(string functionName, string nextSegment)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEnumerable<IEdmFunction> functions = model.SchemaElements.OfType<IEdmFunction>().Where(f => f.Name == functionName);

            // Act
            var functionSegment = FunctionResolver.TryResolveBound(functions, model, nextSegment);

            // Assert
            Assert.Null(functionSegment);
        }

        [Theory]
        [InlineData("(FunctionParameter=true)")]
        [InlineData("(FunctionParameter='abcde')")]
        public void TryResolveBound_ThrowsException_IfMultipleBoundFunctionResolved(string nextSement)
        {
            // Arrange
            const string FunctionName = "BoundOverloadFailed";
            IEdmModel model = GetEdmModel();
            IEnumerable<IEdmFunction> functions = model.SchemaElements.OfType<IEdmFunction>().Where(f => f.Name == FunctionName);

            // Act & Assert
            Assert.Throws<ODataException>(() => FunctionResolver.TryResolveBound(functions, model, nextSement),
                "Function resolution failed. Multiple functions found in the model with identifier: 'BoundOverloadFailed'" +
                " and parameter names : 'FunctionParameter'.");
        }

        [Theory]
        [InlineData("FunctionWithoutParams", "()", "FunctionWithoutParams()")] // function without params using explicit empty parameter list
        [InlineData("FunctionWithoutParams", null, "FunctionWithoutParams()")] // function without params concise invocation
        [InlineData("FunctionWithoutParams", "non parameter list", "FunctionWithoutParams()")] // function without params concise invocation
        [InlineData("FunctionWithOneParam", "(Parameter=1)", "FunctionWithOneParam(Parameter=1)")] // function with one param
        [InlineData("FunctionWithOneParam", "(Parameter=@1)", "FunctionWithOneParam(Parameter=@1)")] // function with one param and aliased value
        [InlineData("FunctionWithOneParam", "(Parameter='1')", "FunctionWithOneParam(Parameter='1')")] // function with one param string value
        [InlineData("FunctionWithMultipleParams", "(Parameter1=1,Parameter2=2,Parameter3=3)", "FunctionWithMultipleParams(Parameter1=1,Parameter2=2,Parameter3=3)")] // function with multiple params
        [InlineData("FunctionWithMultipleParams", "(Parameter2=1,Parameter3=2,Parameter1=3)", "FunctionWithMultipleParams(Parameter2=1,Parameter3=2,Parameter1=3)")] // function with multiple params, different order
        [InlineData("FunctionWithOverloads", "()", "FunctionWithOverloads()")] // overloaded function, empty parameter overload
        [InlineData("FunctionWithOverloads", null, "FunctionWithOverloads()")] // overloaded function, empty parameter overload, concise notation
        [InlineData("FunctionWithOverloads", "(Parameter=1)", "FunctionWithOverloads(Parameter=1)")] // overloaded function, one param
        [InlineData("FunctionWithOverloads", "(Parameter1=1,Parameter2=2,Parameter3=3)", "FunctionWithOverloads(Parameter1=1,Parameter2=2,Parameter3=3)")] // overloaded function, multiple params
        [InlineData("FunctionWithOverloads", "(Parameter2=2,Parameter1=1,Parameter3=3)", "FunctionWithOverloads(Parameter2=2,Parameter1=1,Parameter3=3)")] // overloaded function, multiple params - random order
        public void TryResolveUnbound(string functionName, string nextSegment, string expectedResult)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmFunctionImport> functions =
                container.OperationImports().Where(o => o.Name == functionName).OfType<IEdmFunctionImport>();

            // Act
            UnboundFunctionPathSegment pathSegment = FunctionResolver.TryResolveUnbound(functions, model, nextSegment);

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
        public void TryResolveUnbound_NegativeTests(string functionName, string nextSegment)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmFunctionImport> functions =
                container.OperationImports().Where(o => o.Name == functionName).OfType<IEdmFunctionImport>();

            // Act & Assert
            Assert.Null(FunctionResolver.TryResolveUnbound(functions, model, nextSegment));
        }

        [Theory]
        [InlineData("(FunctionParameter=true)")]
        [InlineData("(FunctionParameter='abcde')")]
        public void TryResolveUnbound_Returns_IfMultipleUnboundFunctionResolved(string nextSement)
        {
            // Arrange
            const string FunctionName = "UnboundOverloadFailed";
            IEdmModel model = GetEdmModel();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmFunctionImport> functions =
                container.OperationImports().Where(o => o.Name == FunctionName).OfType<IEdmFunctionImport>();

            // Act & Assert
            Assert.Throws<ODataException>(() => FunctionResolver.TryResolveUnbound(functions, model, nextSement),
                "Function resolution failed. Multiple functions found in the model with identifier: 'UnboundOverloadFailed'" +
                " and parameter names : 'FunctionParameter'.");
        }

        private IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Name");
            model.AddElement(container);
            var entityType = new EdmEntityType("NS", "EntityTypeName");
            model.AddElement(entityType);
            container.AddEntitySet("EntitySet", entityType);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);

            // unbound function without parameter
            container.AddFunctionImport(new EdmFunction("NS", "FunctionWithoutParams", returnType));

            // unbound function with one parameter
            var functionWithOneParam = new EdmFunction("NS", "FunctionWithOneParam", returnType);
            functionWithOneParam.AddParameter("Parameter", parameterType);
            container.AddFunctionImport(functionWithOneParam);

            // unbound function with multiple parameters
            var functionWithMultipleParams = new EdmFunction("NS", "FunctionWithMultipleParams", returnType);
            functionWithMultipleParams.AddParameter("Parameter1", parameterType);
            functionWithMultipleParams.AddParameter("Parameter2", parameterType);
            functionWithMultipleParams.AddParameter("Parameter3", parameterType);
            container.AddFunctionImport(functionWithMultipleParams);

            // unbound overload function
            container.AddFunctionImport(new EdmFunction("NS", "FunctionWithOverloads", returnType));
            var functionWithOverloads2 = new EdmFunction("NS", "FunctionWithOverloads", returnType);
            functionWithOverloads2.AddParameter("Parameter", parameterType);
            container.AddFunctionImport(functionWithOverloads2);

            var functionWithOverloads3 = new EdmFunction("NS", "FunctionWithOverloads", returnType);
            functionWithOverloads3.AddParameter("Parameter1", parameterType);
            functionWithOverloads3.AddParameter("Parameter2", parameterType);
            functionWithOverloads3.AddParameter("Parameter3", parameterType);
            container.AddFunctionImport(functionWithOverloads3);

            // bound function with only binding parameter
            IEdmTypeReference bindingParamterType = new EdmEntityTypeReference(entityType, isNullable: false);
            var boundFunction = new EdmFunction("NS", "BoundFunctionWithoutParams", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            model.AddElement(boundFunction);

            // bound function with binding and one non-binding parameter
            boundFunction = new EdmFunction("NS", "BoundFunctionWithOneParam", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("Parameter", parameterType);
            model.AddElement(boundFunction);

            // bound function with binding and multiple non-binding parameters
            boundFunction = new EdmFunction("NS", "BoundFunctionWithMultipleParams", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("Parameter1", parameterType);
            boundFunction.AddParameter("Parameter2", parameterType);
            boundFunction.AddParameter("Parameter3", parameterType);
            model.AddElement(boundFunction);

            // bound overload function with only binding parameter
            boundFunction = new EdmFunction("NS", "BoundFunctionOverload", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            model.AddElement(boundFunction);

            // bound overload function with binding and one non-binding parameter
            boundFunction = new EdmFunction("NS", "BoundFunctionOverload", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("Parameter", parameterType);
            model.AddElement(boundFunction);

            // bound overload function with binding and multiple non-binding parameters
            boundFunction = new EdmFunction("NS", "BoundFunctionOverload", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("Parameter1", parameterType);
            boundFunction.AddParameter("Parameter2", parameterType);
            boundFunction.AddParameter("Parameter3", parameterType);
            model.AddElement(boundFunction);

            // the following two bound functions have the same function name, same binding type and the same parameter name,
            // but the non-binding parameter type is different.
            boundFunction = new EdmFunction("NS", "BoundOverloadFailed", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("FunctionParameter", parameterType);
            model.AddElement(boundFunction);

            boundFunction = new EdmFunction("NS", "BoundOverloadFailed", returnType, true, null, true);
            boundFunction.AddParameter("bindingParameter", bindingParamterType);
            boundFunction.AddParameter("FunctionParameter", stringType);
            model.AddElement(boundFunction);

            // the following two unbound functions have the same function name and the same parameter name,
            // but the parameter type is different.
            var function = new EdmFunction("NS", "UnboundOverloadFailed", returnType);
            function.AddParameter("FunctionParameter", parameterType);
            container.AddFunctionImport(function);

            function = new EdmFunction("NS", "UnboundOverloadFailed", returnType);
            function.AddParameter("FunctionParameter", stringType);
            container.AddFunctionImport(function);

            return model;
        }
    }
}
