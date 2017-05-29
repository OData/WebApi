// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class ODataPathSegmentTranslatorTest
    {
        private readonly IEdmModel _model;
        private ODataPathSegmentTranslator _translator;

        public ODataPathSegmentTranslatorTest()
        {
            _model = ODataRoutingModel.GetModel();
            _translator = new ODataPathSegmentTranslator(_model, false, new Dictionary<string, SingleValueNode>());
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_IfMissModel()
        {
            Assert.ThrowsArgumentNull(() => new ODataPathSegmentTranslator(model: null, enableUriTemplateParsing: false,
                parameterAliasNodes: null), "model");
        }

        [Fact]
        public void Ctor_ThrowsArgumentException_IfMissParameterAliasNodes()
        {
            Assert.ThrowsArgumentNull(() => new ODataPathSegmentTranslator(new EdmModel(),
                enableUriTemplateParsing: false, parameterAliasNodes: null), "parameterAliasNodes");
        }

        [Fact]
        public void Translate_TypeSegment_To_CastPathSegment_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("RoutingCustomers");
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            TypeSegment segment = new TypeSegment(entityType, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            CastPathSegment castPathSegment = Assert.IsType<CastPathSegment>(pathSegment);
            Assert.Same(entityType, castPathSegment.CastType);
        }

        [Fact]
        public void Translate_EntitySetSegment_To_EntitySetPathSegment_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("RoutingCustomers");
            EntitySetSegment segment = new EntitySetSegment(entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            EntitySetPathSegment entitySetPathSegment = Assert.IsType<EntitySetPathSegment>(pathSegment);
            Assert.Same(entityset, entitySetPathSegment.EntitySetBase);
        }

        [Fact]
        public void Translate_SingletonSegment_To_SingletonPathSegment_Works()
        {
            // Arrange
            IEdmSingleton singleton = _model.FindDeclaredSingleton("VipCustomer");
            SingletonSegment segment = new SingletonSegment(singleton);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            SingletonPathSegment singletonPathSegment = Assert.IsType<SingletonPathSegment>(pathSegment);
            Assert.Same(singleton, singletonPathSegment.Singleton);
        }

        [Fact]
        public void Translate_NavigationPropertySegment_To_NavigationPathSegment_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("Products");
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            IEdmNavigationProperty navigationProperty = entityType.NavigationProperties().First(e => e.Name == "Products");
            NavigationPropertySegment segment = new NavigationPropertySegment(navigationProperty, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            NavigationPathSegment navigationPathSegment = Assert.IsType<NavigationPathSegment>(pathSegment);
            Assert.Same(navigationProperty, navigationPathSegment.NavigationProperty);
        }

        [Fact]
        public void Translate_KeySegment_To_KeyValuePathSegment_Works()
        {
            // Arrange
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("RoutingCustomers");
            KeySegment segment = new KeySegment(new[] { new KeyValuePair<string, object>("ID", 42) }, entityType, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            KeyValuePathSegment keyValuePathSegment = Assert.IsType<KeyValuePathSegment>(pathSegment);
            Assert.Equal("42", keyValuePathSegment.Value);
        }

        [Fact]
        public void Translate_PropertySegment_To_PropertyAccessPathSegment_Works()
        {
            // Arrange
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            IEdmStructuralProperty property = entityType.FindProperty("Name") as IEdmStructuralProperty;
            PropertySegment segment = new PropertySegment(property);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            PropertyAccessPathSegment propertyPathSegment = Assert.IsType<PropertyAccessPathSegment>(pathSegment);
            Assert.Same(property, propertyPathSegment.Property);
        }

        [Fact]
        public void Translate_OperationImportSegment_To_UnboundActionPathSegment_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("SalesPeople");
            IEnumerable<IEdmOperationImport> operationImports = _model.FindDeclaredOperationImports("GetSalesPersonById");
            OperationImportSegment segment = new OperationImportSegment(operationImports, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            UnboundActionPathSegment unboundActionPathSegment = Assert.IsType<UnboundActionPathSegment>(pathSegment);
            Assert.Same(operationImports.First(), unboundActionPathSegment.Action);
        }

        [Fact]
        public void Translate_OperationSegment_To_BoundActionPathSegment_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("Products");
            IEnumerable<IEdmOperation> operations = _model.FindDeclaredOperations("Default.GetProducts");
            OperationSegment segment = new OperationSegment(operations, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            BoundActionPathSegment boundActionPathSegment = Assert.IsType<BoundActionPathSegment>(pathSegment);
            Assert.Same(operations.First(), boundActionPathSegment.Action);
        }

        [Fact]
        public void Translate_OpenPropertySegment_To_DynamicPropertyPathSegment_Works()
        {
            // Arrange
            OpenPropertySegment segment = new OpenPropertySegment("Dynamic");

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            DynamicPropertyPathSegment dynamicPropertyPathSegment = Assert.IsType<DynamicPropertyPathSegment>(pathSegment);
            Assert.Equal("Dynamic", dynamicPropertyPathSegment.PropertyName);
        }

        [Fact]
        public void Translate_CountSegment_To_CountPathSegment_Works()
        {
            // Arrange
            CountSegment segment = CountSegment.Instance;

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            Assert.IsType<CountPathSegment>(pathSegment);
        }

        [Fact]
        public void Translate_NavigationPropertyLinkSegment_To_RefPathSegments_Works()
        {
            // Arrange
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("Products");
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            IEdmNavigationProperty navigationProperty = entityType.NavigationProperties().First(e => e.Name == "Products");
            NavigationPropertyLinkSegment segment = new NavigationPropertyLinkSegment(navigationProperty, entityset);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            Assert.Equal(2, segments.Count());
            NavigationPathSegment navigationPathSegment = Assert.IsType<NavigationPathSegment>(segments.First());
            Assert.Same(navigationProperty, navigationPathSegment.NavigationProperty);

            Assert.IsType<RefPathSegment>(segments.Last());
        }

        [Fact]
        public void Translate_ValueSegment_To_ValuePathSegment_Works()
        {
            // Arrange
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            ValueSegment segment = new ValueSegment(entityType);

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            Assert.IsType<ValuePathSegment>(pathSegment);
        }

        [Fact]
        public void Translate_BatchSegment_To_BatchPathSegment_Works()
        {
            // Arrange
            BatchSegment segment = BatchSegment.Instance;

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            Assert.IsType<BatchPathSegment>(pathSegment);
        }

        [Fact]
        public void Translate_BatchReferenceSegment_Throws()
        {
            // Arrange
            IEdmEntityType entityType = _model.FindDeclaredType("System.Web.OData.Routing.RoutingCustomer") as IEdmEntityType;
            IEdmEntitySet entityset = _model.FindDeclaredEntitySet("RoutingCustomers");
            BatchReferenceSegment segment = new BatchReferenceSegment("$10", entityType, entityset);

            // Act & Assert
            Assert.Throws<ODataException>(() => _translator.Translate(segment), "'ODataPathSegment' of kind 'BatchReferenceSegment' is not implemented.");
        }

        [Fact]
        public void Translate_MetadataSegment_To_MetadataPathSegment_Works()
        {
            // Arrange
            MetadataSegment segment = MetadataSegment.Instance;

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            Assert.IsType<MetadataPathSegment>(pathSegment);
        }

        [Fact]
        public void Translate_PathTemplateSegment_To_DynamicPropertyPathSegment_Works()
        {
            // Arrange
            PathTemplateSegment segment = new PathTemplateSegment("{pName:dynamicproperty}");

            // Act
            IEnumerable<ODataPathSegment> segments = _translator.Translate(segment);

            // Assert
            ODataPathSegment pathSegment = Assert.Single(segments);
            DynamicPropertyPathSegment dynamicSegment = Assert.IsType<DynamicPropertyPathSegment>(pathSegment);
            Assert.Equal("{pName}", dynamicSegment.PropertyName);
        }
    }
}
