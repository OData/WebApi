// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class ProcedureHelpersTest
    {
        private IEdmEntityTypeReference _entityType;
        private IEdmEntityTypeReference _derivedEntityType;
        private IEdmModel _model;
        private IEdmEntityContainer _container;

        public ProcedureHelpersTest()
        {
            BuildEdmModel();
        }

        [Theory]
        [InlineData("NonBindableAction")]
        public void FindActionImport_CanFind_NonbindableAction(string segment)
        {
            // Arrange & Act
            var result = _container.FindActionImport(segment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.Name.NonBindableAction", result.Container.FullName() + "." + result.Name);
        }

        [Theory]
        [InlineData("NS.ActionBoundToEntity", true)]
        [InlineData("NS.ActionBoundToEntity", false)]
        public void FindAction_CanFind_BindableAction_Entity(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmType bindingParameterType = isDerivedType ? _derivedEntityType.Definition : _entityType.Definition;

            // Act
            var result = _model.FindAction(segment, bindingParameterType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.ActionBoundToEntity", result.FullName());
        }

        [Fact]
        public void FindAction_CannotFind_BindableAction_DerivedEntity()
        {
            // Act & Assert
            Assert.Null(_model.FindAction("NS.ActionBoundToDerivedEntity", _entityType.Definition));
        }

        [Fact]
        public void FindAction_CannotFind_BindableAction_DerivedEntityCollection()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);

            // Act & Assert
            Assert.Null(_model.FindAction("NS.ActionBoundToDerivedEntityCollection", entityCollection));
        }

        [Theory]
        [InlineData("NS.ActionBoundToEntityCollection", true)]
        [InlineData("NS.ActionBoundToEntityCollection", false)]
        public void FindAction_CanFind_BindableAction_EntityCollection(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmTypeReference entityType = isDerivedType ? _derivedEntityType : _entityType;
            var entityCollection = new EdmCollectionType(entityType);

            // Act
            var result = _model.FindAction(segment, entityCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NS.ActionBoundToEntityCollection", result.FullName());
        }

        [Fact]
        public void FindActionImport_Throws_ActionResolutionFailed_AmbiguosAction()
        {
            // Act
            Assert.ThrowsArgument(
                () => _container.FindActionImport("AmbiguousAction"),
                "actionIdentifier",
                "Action resolution failed. Multiple actions matching the action identifier 'AmbiguousAction' were found. " +
                "The matching actions are: AmbiguousAction, AmbiguousAction.");
        }

        [Theory]
        [InlineData("NonBindableFunction")]
        public void FindMatchedOperationImports_CanFind_NonbindableFunctions(string segment)
        {
            // Act
            var results = _container.FindMatchedOperationImports(segment).OfType<IEdmFunctionImport>();

            // Assert
            Assert.Equal(new[] { "NS.Name.NonBindableFunction" }, results.Select(f => f.Container.FullName() + "." + f.Name));
        }

        [Theory]
        [InlineData("NS.FunctionBoundToEntity", true)]
        [InlineData("NS.FunctionBoundToEntity", false)]
        public void FindMatchedOperations_CanFind_BindableFunctions_Entity(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmType bindingParameterType = isDerivedType ? _derivedEntityType.Definition : _entityType.Definition;

            // Act
            var results = _model.FindMatchedOperations(segment, bindingParameterType);

            // Assert
            Assert.Equal(new[] { "NS.FunctionBoundToEntity" }, results.Select(f => f.FullName()));
        }

        [Theory]
        [InlineData("NS.FunctionBoundToEntityCollection", true)]
        [InlineData("NS.FunctionBoundToEntityCollection", false)]
        public void FindMatchedOpeartions_CanFind_BindableFunctions_EntityCollection(string segment, bool isDerivedType)
        {
            // Arrange
            IEdmTypeReference entityType = isDerivedType ? _derivedEntityType : _entityType;
            var entityCollection = new EdmCollectionType(entityType);

            // Act
            var results = _model.FindMatchedOperations(segment, entityCollection);

            // Assert
            Assert.Equal(new[] { "NS.FunctionBoundToEntityCollection" }, results.Select(f => f.FullName()));
        }

        [Fact]
        public void FindMatchedOperations_CannotFind_BindableFunction_DerivedEntity()
        {
            // Act & Assert
            Assert.Empty(_model.FindMatchedOperations("NS.FunctionBoundToDerivedEntity",  _entityType.Definition));
        }

        [Fact]
        public void FindMatchedOperations_CannotFind_BindableFunction_DerivedEntityCollection()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);

            // Act & Assert
            Assert.Empty(_model.FindMatchedOperations("NS.FunctionBoundToDerivedEntityCollection",  entityCollection));
        }

        [Fact]
        public void FindMatchedOperations_DoesNotReturnNonBindableFunction_IfBindingParameterSpecified()
        {
            // Arrange
            var entityCollection = new EdmCollectionType(_entityType);

            // Act & Assert
            Assert.Empty(_model.FindMatchedOperations("NS.NonBindableAction", entityCollection));
            Assert.Empty(_model.FindMatchedOperations("NS.NonBindableAction", _entityType.Definition));
        }

        private void BuildEdmModel()
        {
            _entityType = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), isNullable: false);
            _derivedEntityType = new EdmEntityTypeReference(new EdmEntityType("NS", "DerivedEntity", _entityType.EntityDefinition()), isNullable: false);
            var entityCollection = new EdmCollectionTypeReference(new EdmCollectionType(_entityType));
            var derivedEntityCollection = new EdmCollectionTypeReference(new EdmCollectionType(_derivedEntityType));

            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Name");
            model.AddElement(container);

            // non-bindable action
            container.AddActionImport(new EdmAction("NS", "NonBindableAction", returnType: null));

            // action bound to entity
            var actionBoundToEntity = new EdmAction(
                "NS",
                "ActionBoundToEntity",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToEntity.AddParameter("bindingParameter", _entityType);
            model.AddElement(actionBoundToEntity);

            // action bound to derived entity
            var actionBoundToDerivedEntity = new EdmAction(
                "NS",
                "ActionBoundToDerivedEntity",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToDerivedEntity.AddParameter("bindingParameter", _derivedEntityType);
            model.AddElement(actionBoundToDerivedEntity);

            // action bound to entity collection
            var actionBoundToEntityCollection = new EdmAction(
                "NS",
                "ActionBoundToEntityCollection",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToEntityCollection.AddParameter("bindingParameter", entityCollection);
            model.AddElement(actionBoundToEntityCollection);

            // action bound to derived entity collection
            var actionBoundToDerivedEntityCollection = new EdmAction(
                "NS",
                "ActionBoundToDerivedEntityCollection",
                returnType: null,
                isBound: true,
                entitySetPathExpression: null);
            actionBoundToDerivedEntityCollection.AddParameter("bindingParameter", derivedEntityCollection);
            model.AddElement(actionBoundToDerivedEntityCollection);

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
            functionBoundToEntity.AddParameter("bindingParameter", _entityType);
            model.AddElement(functionBoundToEntity);

            // function bound to entity
            var functionBoundToDerivedEntity = new EdmFunction(
                "NS",
                "FunctionBoundToDerivedEntity",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToDerivedEntity.AddParameter("bindingParameter", _derivedEntityType);
            model.AddElement(functionBoundToDerivedEntity);

            // function bound to entity collection
            var functionBoundToEntityCollection = new EdmFunction(
                "NS",
                "FunctionBoundToEntityCollection",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToEntityCollection.AddParameter("bindingParameter", entityCollection);
            model.AddElement(functionBoundToEntityCollection);

            // function bound to derived entity collection
            var functionBoundToDerivedEntityCollection = new EdmFunction(
                "NS",
                "FunctionBoundToDerivedEntityCollection",
                returnType,
                isBound: true,
                entitySetPathExpression: null,
                isComposable: false);
            functionBoundToDerivedEntityCollection.AddParameter("bindingParameter", derivedEntityCollection);
            model.AddElement(functionBoundToDerivedEntityCollection);

            _model = model;
            _container = container;
        }
    }
}
