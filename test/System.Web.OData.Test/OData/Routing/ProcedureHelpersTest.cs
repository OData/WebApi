// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
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
            container.AddFunctionImport("NonBindableAction", returnType: null, entitySet: null, sideEffecting: true,
                composable: false, bindable: false);

            // action bound to entity
            var actionBoundToEntity = container.AddFunctionImport("ActionBoundToEntity", returnType: null, entitySet: null,
                sideEffecting: true, composable: false, bindable: true);
            actionBoundToEntity.AddParameter("Param", _entityType);

            // action bound to derived entity
            var actionBoundToDerivedEntity = container.AddFunctionImport("ActionBoundToDerivedEntity", returnType: null,
                entitySet: null, sideEffecting: true, composable: false, bindable: true);
            actionBoundToDerivedEntity.AddParameter("Param", _derivedEntityType);

            // action bound to entity collection
            var actionBoundToEntityCollection = container.AddFunctionImport("ActionBoundToEntityCollection", returnType: null,
                entitySet: null, sideEffecting: true, composable: false, bindable: true);
            actionBoundToEntityCollection.AddParameter("Param", entityCollection);

            // action bound to derived entity collection
            var actionBoundToDerivedEntityCollection = container.AddFunctionImport("ActionBoundToDerivedEntityCollection",
                returnType: null, entitySet: null, sideEffecting: true, composable: false, bindable: true);
            actionBoundToDerivedEntityCollection.AddParameter("Param", derivedEntityCollection);

            // ambiguos actions
            container.AddFunctionImport("AmbiguousAction", returnType: null, entitySet: null, sideEffecting: true,
                composable: false, bindable: false);
            container.AddFunctionImport("AmbiguousAction", returnType: null, entitySet: null, sideEffecting: true,
                composable: false, bindable: false);

            // non-bindable function
            container.AddFunctionImport("NonBindableFunction", returnType: null, entitySet: null, sideEffecting: false,
                composable: false, bindable: false);

            // function bound to entity
            var functionBoundToEntity = container.AddFunctionImport("FunctionBoundToEntity", returnType: null, entitySet: null,
                sideEffecting: false, composable: false, bindable: true);
            functionBoundToEntity.AddParameter("Param", _entityType);

            // function bound to entity
            var functionBoundToDerivedEntity = container.AddFunctionImport("FunctionBoundToDerivedEntity", returnType: null, entitySet: null,
                sideEffecting: false, composable: false, bindable: true);
            functionBoundToDerivedEntity.AddParameter("Param", _derivedEntityType);

            // function bound to entity collection
            var functionBoundToEntityCollection = container.AddFunctionImport("FunctionBoundToEntityCollection", returnType: null,
                entitySet: null, sideEffecting: false, composable: false, bindable: true);
            functionBoundToEntityCollection.AddParameter("Param", entityCollection);

            // function bound to derived entity collection
            var functionBoundToDerivedEntityCollection = container.AddFunctionImport("FunctionBoundToDerivedEntityCollection", returnType: null,
                entitySet: null, sideEffecting: false, composable: false, bindable: true);
            functionBoundToDerivedEntityCollection.AddParameter("Param", derivedEntityCollection);

            return container;
        }
    }
}
