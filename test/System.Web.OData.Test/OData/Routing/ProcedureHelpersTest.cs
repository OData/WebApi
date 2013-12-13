// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ProcedureHelpersTest
    {
        private static IEdmEntityTypeReference _entityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);
        private static IEdmEntityTypeReference _derivedEntityType =
            new EdmEntityTypeReference(new EdmEntityType("NS", "DerivedEntity", _entityType.EntityDefinition()), isNullable: false);

        [Theory]
        [InlineData("NonBindableAction")]
        [InlineData("Name.NonBindableAction")]
        [InlineData("NS.Name.NonBindableAction")]
        public void FindAction_CanFind_NonBindableAction(string segment)
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();

            // Act
            var result = container.FindAction(segment, bindingParameterType: null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.Name.NonBindableAction", result.Container.FullName() + "." + result.Name);
        }

        [Theory]
        [InlineData("ActionBoundToEntity", true)]
        [InlineData("Name.ActionBoundToEntity", true)]
        [InlineData("NS.Name.ActionBoundToEntity", true)]
        [InlineData("ActionBoundToEntity", false)]
        [InlineData("Name.ActionBoundToEntity", false)]
        [InlineData("NS.Name.ActionBoundToEntity", false)]
        public void FindAction_CanFind_BindableAction_Entity(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();
            IEdmType bindingParameterType = isDerivedType ? _derivedEntityType.Definition : _entityType.Definition;

            // Act
            var result = container.FindAction(segment, bindingParameterType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.Name.ActionBoundToEntity", result.Container.FullName() + "." + result.Name);
        }

        [Fact]
        public void FindAction_CannotFind_BindableAction_DerivedEntity()
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();

            // Act & Assert
            Assert.Null(container.FindAction("ActionBoundToDerivedEntity", bindingParameterType: _entityType.Definition));
        }

        [Fact]
        public void FindAction_CannotFind_BindableAction_DerivedEntityCollection()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);
            IEdmEntityContainer container = GetEntityContainer();

            // Act & Assert
            Assert.Null(container.FindAction("ActionBoundToDerivedEntityCollection", bindingParameterType: entityCollection));
        }

        [Theory]
        [InlineData("ActionBoundToEntityCollection", true)]
        [InlineData("Name.ActionBoundToEntityCollection", true)]
        [InlineData("NS.Name.ActionBoundToEntityCollection", true)]
        [InlineData("ActionBoundToEntityCollection", false)]
        [InlineData("Name.ActionBoundToEntityCollection", false)]
        [InlineData("NS.Name.ActionBoundToEntityCollection", false)]
        public void FindAction_CanFind_BindableAction_EntityCollection(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmTypeReference entityType = isDerivedType ? _derivedEntityType : _entityType;
            var entityCollection = new EdmCollectionType(entityType);
            IEdmEntityContainer container = GetEntityContainer();

            // Act
            var result = container.FindAction(segment, bindingParameterType: entityCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.Name.ActionBoundToEntityCollection", result.Container.FullName() + "." + result.Name);
        }

        [Fact]
        public void FindAction_Throws_ActionResolutionFailed_AmbiguosAction()
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();

            // Act
            Assert.ThrowsArgument(
                () => container.FindAction("AmbiguousAction", bindingParameterType: null),
                "actionIdentifier",
                "Action resolution failed. Multiple actions matching the action identifier 'AmbiguousAction' were found. " +
                "The matching actions are: NS.Name.AmbiguousAction, NS.Name.AmbiguousAction.");
        }

        [Theory]
        [InlineData("NonBindableFunction")]
        [InlineData("Name.NonBindableFunction")]
        [InlineData("NS.Name.NonBindableFunction")]
        public void FindFunctions_CanFind_NonBindableFunctions(string segment)
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();

            // Act
            var results = container.FindFunctions(segment, bindingParameterType: null);

            // Assert
            Assert.Equal(new[] { "NS.Name.NonBindableFunction" }, results.Select(f => f.Container.FullName() + "." + f.Name));
        }

        [Theory]
        [InlineData("FunctionBoundToEntity", true)]
        [InlineData("Name.FunctionBoundToEntity", true)]
        [InlineData("NS.Name.FunctionBoundToEntity", true)]
        [InlineData("FunctionBoundToEntity", false)]
        [InlineData("Name.FunctionBoundToEntity", false)]
        [InlineData("NS.Name.FunctionBoundToEntity", false)]
        public void FindFunctions_CanFind_BindableFunctions_Entity(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();
            IEdmType bindingParameterType = isDerivedType ? _derivedEntityType.Definition : _entityType.Definition;

            // Act
            var results = container.FindFunctions(segment, bindingParameterType);

            // Assert
            Assert.Equal(new[] { "NS.Name.FunctionBoundToEntity" }, results.Select(f => f.Container.FullName() + "." + f.Name));
        }

        [Theory]
        [InlineData("FunctionBoundToEntityCollection", true)]
        [InlineData("Name.FunctionBoundToEntityCollection", true)]
        [InlineData("NS.Name.FunctionBoundToEntityCollection", true)]
        [InlineData("FunctionBoundToEntityCollection", false)]
        [InlineData("Name.FunctionBoundToEntityCollection", false)]
        [InlineData("NS.Name.FunctionBoundToEntityCollection", false)]
        public void FindFunctions_CanFind_BindableFunctions_EntityCollection(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmTypeReference entityType = isDerivedType ? _derivedEntityType : _entityType;
            var entityCollection = new EdmCollectionType(entityType);
            IEdmEntityContainer container = GetEntityContainer();

            // Act
            var results = container.FindFunctions(segment, bindingParameterType: entityCollection);

            // Assert
            Assert.Equal(new[] { "NS.Name.FunctionBoundToEntityCollection" }, results.Select(f => f.Container.FullName() + "." + f.Name));
        }

        [Fact]
        public void FindFunctions_CannotFind_BindableFunction_DerivedEntity()
        {
            // Arrange
            IEdmEntityContainer container = GetEntityContainer();

            // Act & Assert
            Assert.Empty(container.FindFunctions("FunctionBoundToDerivedEntity", bindingParameterType: _entityType.Definition));
        }

        [Fact]
        public void FindFunctions_CannotFind_BindableFunction_DerivedEntityCollection()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);
            IEdmEntityContainer container = GetEntityContainer();

            // Act & Assert
            Assert.Empty(container.FindFunctions("FunctionBoundToDerivedEntityCollection", bindingParameterType: entityCollection));
        }

        [Fact]
        public void FindFunctions_DoesNotReturnNonBindableFunction_IfBindingParameterSpecified()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);
            IEdmEntityContainer container = GetEntityContainer();

            // Act & Assert
            Assert.Empty(container.FindFunctions("NonBindableAction", bindingParameterType: entityCollection));
            Assert.Empty(container.FindFunctions("NonBindableAction", bindingParameterType: _entityType.Definition));
        }

        private static IEdmEntityContainer GetEntityContainer()
        {
            var entityCollection = new EdmCollectionTypeReference(new EdmCollectionType(_entityType), isNullable: false);
            var derivedEntityCollection = new EdmCollectionTypeReference(new EdmCollectionType(_derivedEntityType), isNullable: false);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Name");

            // non-bindable action
            container.AddActionImport(new EdmAction("NS", "NonBindableAction", returnType: null));

            // action bound to entity
            var actionBoundToEntity = new EdmAction(
                "NS",
                "ActionBoundToEntity",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToEntity.AddParameter("Param", _entityType);
            container.AddActionImport(actionBoundToEntity);

            // action bound to derived entity
            var actionBoundToDerivedEntity = new EdmAction(
                "NS",
                "ActionBoundToDerivedEntity",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToDerivedEntity.AddParameter("Param", _derivedEntityType);
            container.AddActionImport(actionBoundToDerivedEntity);

            // action bound to entity collection
            var actionBoundToEntityCollection = new EdmAction(
                "NS",
                "ActionBoundToEntityCollection",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToEntityCollection.AddParameter("Param", entityCollection);
            container.AddActionImport(actionBoundToEntityCollection);

            // action bound to derived entity collection
            var actionBoundToDerivedEntityCollection = new EdmAction(
                "NS",
                "ActionBoundToDerivedEntityCollection",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToDerivedEntityCollection.AddParameter("Param", derivedEntityCollection);
            container.AddActionImport(actionBoundToDerivedEntityCollection);

            // ambiguos actions
            container.AddActionImport(new EdmAction("NS", "AmbiguousAction", returnType: null));
            container.AddActionImport(new EdmAction("NS", "AmbiguousAction", returnType: null));

            IEdmTypeReference returnType = new EdmPrimitiveTypeReference(
                EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32), false);

            // non-bindable function
            container.AddFunctionImport(new EdmFunction("NS", "NonBindableFunction", returnType));

            // function bound to entity
            var functionBoundToEntity = new EdmFunction(
                "NS",
                "FunctionBoundToEntity",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToEntity.AddParameter("Param", _entityType);
            container.AddFunctionImport(functionBoundToEntity);

            // function bound to entity
            var functionBoundToDerivedEntity = new EdmFunction(
                "NS",
                "FunctionBoundToDerivedEntity",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToDerivedEntity.AddParameter("Param", _derivedEntityType);
            container.AddFunctionImport(functionBoundToDerivedEntity);

            // function bound to entity collection
            var functionBoundToEntityCollection = new EdmFunction(
                "NS",
                "FunctionBoundToEntityCollection",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToEntityCollection.AddParameter("Param", entityCollection);
            container.AddFunctionImport(functionBoundToEntityCollection);

            // function bound to derived entity collection
            var functionBoundToDerivedEntityCollection = new EdmFunction(
                "NS",
                "FunctionBoundToDerivedEntityCollection",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToDerivedEntityCollection.AddParameter("Param", derivedEntityCollection);
            container.AddFunctionImport(functionBoundToDerivedEntityCollection);

            return container;
        }
    }
}
