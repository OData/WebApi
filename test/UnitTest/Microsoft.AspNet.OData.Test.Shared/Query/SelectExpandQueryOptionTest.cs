// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using Customer = Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.Customer;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class SelectExpandQueryOptionTest
    {
        private CustomersModelWithInheritance _model = new CustomersModelWithInheritance();

        [Fact]
        public void Ctor_ThrowsArgumentNull_Context()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new SelectExpandQueryOption(select: "select", expand: "expand", context: null),
                "context");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfBothSelectAndExpandAreNull()
        {
            // Arrange
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(Customer));

            // Act & Assert
            ExceptionAssert.Throws<ArgumentException>(
                () => new SelectExpandQueryOption(select: null, expand: null, context: context),
                "'select' and 'expand' cannot be both null or empty.");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfContextIsNotForStructuredType()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(int));

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => new SelectExpandQueryOption(select: "Name", expand: "Name", context: context),
                "context",
                "The type 'Edm.Int32' is not a structured type. Only structured types support $select and $expand.");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_QueryOptionParser()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(int));

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SelectExpandQueryOption("select", "expand", context, queryOptionParser: null),
                "queryOptionParser");
        }

        [Fact]
        public void Ctor_SetsProperty_RawSelect()
        {
            // Arrange
            string selectValue = "select";
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(Customer));
            context.RequestContainer = new MockContainer();

            // Act
            SelectExpandQueryOption result = new SelectExpandQueryOption(selectValue, expand: null, context: context);

            // Assert
            Assert.Equal(selectValue, result.RawSelect);
        }

        [Fact]
        public void Ctor_SetsProperty_RawExpand()
        {
            // Arrange
            string expandValue = "expand";
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(Customer));
            context.RequestContainer = new MockContainer();

            // Act
            SelectExpandQueryOption result = new SelectExpandQueryOption(select: null, expand: expandValue, context: context);

            // Assert
            Assert.Equal(expandValue, result.RawExpand);
        }

        [Fact]
        public void SelectExpandClause_Property_ParsesRawSelectAndRawExpand()
        {
            // Arrange
            IEdmModel model = _model.Model;
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption option = new SelectExpandQueryOption("ID,Name,SimpleEnum,Orders", "Orders", context);

            // Act
            SelectExpandClause selectExpandClause = option.SelectExpandClause;

            // Assert
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<PathSelectItem>());
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
        }

        [Fact]
        public void SelectExpandClause_Property_ParsesWithNavigationSource()
        {
            // Arrange
            IEdmModel model = _model.Model;
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataPath odataPath = new ODataPath(new EntitySetSegment(_model.Customers));
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), odataPath);
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption option = new SelectExpandQueryOption("ID,Name,SimpleEnum,Orders", "Orders", context);

            // Act
            SelectExpandClause selectExpandClause = option.SelectExpandClause;

            // Assert
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<PathSelectItem>());
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
        }

        [Theory]
        [InlineData("ID,Name,SimpleEnum,Orders", "Orders")]
        [InlineData("iD,NaMe,SiMpLeEnUm,OrDeRs", "OrDeRs")]
        public void SelectExpandClause_Property_ParsesWithEdmTypeAndNavigationSource(string select, string expand)
        {
            // Arrange
            IEdmModel model = _model.Model;
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataPath odataPath = new ODataPath(new EntitySetSegment(_model.Customers));
            ODataQueryContext context = new ODataQueryContext(model, _model.Customer, odataPath);
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption option = new SelectExpandQueryOption(select, expand, context);

            // Act
            SelectExpandClause selectExpandClause = option.SelectExpandClause;

            // Assert
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<PathSelectItem>());
            IEnumerable<string> ids = selectExpandClause.SelectedItems.OfType<PathSelectItem>()
                .Select(p => (p.SelectedPath.FirstSegment as PropertySegment)?.Identifier);
            Assert.Equal(new []{"ID", "Name", "SimpleEnum"}, ids.OfType<string>().ToArray());

            IEnumerable<ExpandedNavigationSelectItem> expands = selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>();
            Assert.NotEmpty(expands);
            Assert.Equal("Orders", expands.Single().NavigationSource.Name);
        }

        [Theory]
        [InlineData("ID", null)]
        [InlineData("LastName,FirstName", null)]
        [InlineData("LastName,    FirstName", null)]
        [InlineData("LastName,FirstName", "Orders")]
        [InlineData("LastName,FirstName,Orders", "Orders")]
        [InlineData("Orders", "Orders,Orders($expand=Customer),Orders($expand=Customer($expand=Orders))")]
        [InlineData("SimpleEnum", "Orders")]
        public void SelectExpandClause_CanParse_ModelBuiltForQueryable(string select, string expand)
        {
            // Arrange
            var config = RoutingConfigurationFactory.Create();
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create(config, isQueryCompositionMode: true);
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption option = new SelectExpandQueryOption(select, expand, context);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => option.SelectExpandClause.ToString());
        }

        [Theory]
        [InlineData("IDD", null, "Could not find a property named 'IDD' on type 'NS.Customer'.")]
        [InlineData("ID, Namee", null, "Could not find a property named 'Namee' on type 'NS.Customer'.")]
        [InlineData("NS+Name", null, "Syntax error: character '+' is not valid at position 2 in 'NS+Name'.")]
        [InlineData("NS.Customerrr/SpecialCustomerProperty", null, "The type 'NS.Customerrr' is not defined in the model.")]
        public void SelectExpandCaluse_ThrowsODataException_InvalidQuery(string select, string expand, string error)
        {
            // Arrange
            IEdmModel model = _model.Model;
            _model.Model.SetAnnotationValue<ClrTypeAnnotation>(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption option = new SelectExpandQueryOption(select, expand, context);

            // Act
            ExceptionAssert.Throws<ODataException>(
                () => option.SelectExpandClause.ToString(),
                error);
        }

        [Fact]
        public void Property_SelectExpandClause_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: context);

            // Act & Assert
            Assert.NotNull(selectExpand.SelectExpandClause);
        }

        [Fact]
        public void ApplyTo_OnQueryable_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => selectExpand.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Fact]
        public void ApplyTo_OnSingleEntity_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            context.RequestContainer = new MockContainer();
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: context);
            object entity = new object();

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => selectExpand.ApplyTo(entity, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ProcessLevelsCorrectly_PreserveOtherOptions(bool autoSelect)
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var entityType = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity");
            var context = new ODataQueryContext(
                model,
                entityType);

            if (autoSelect)
            {
                var modelBound = model.GetAnnotationValue<ModelBoundQuerySettings>(entityType) ?? new ModelBoundQuerySettings();
                modelBound.DefaultSelectType = SelectExpandType.Automatic;
                model.SetAnnotationValue(entityType, modelBound);
            }

            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($filter=Cnt gt 1;$apply=aggregate($count as Cnt))",
                context: context);
            selectExpand.LevelsMaxLiteralExpansionDepth = 1;

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            var item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.NotNull(item.FilterOption);
            Assert.NotNull(item.ApplyOption);
        }


        [Fact]
        public void ProcessLevelsCorrectly_AllSelected()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity"));
            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=Parent($levels=2))",
                context: context);
            selectExpand.LevelsMaxLiteralExpansionDepth = 3;

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            // Level 1.
            Assert.True(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            var item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            // Level 2.
            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            // Level 3.
            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Empty(clause.SelectedItems);
        }

        [Fact]
        public void ProcessLevelsCorrectly_NotAllSelected()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity"));
            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: "Name",
                expand: "Parent($select=ID;$levels=max)",
                context: context);

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            // Level 1.
            Assert.False(clause.AllSelected);
            Assert.Equal(2, clause.SelectedItems.Count());

            var nameSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is PropertySegment));
            Assert.Equal("Name", ((PropertySegment)nameSelectItem.SelectedPath.FirstSegment).Property.Name);

            // Before ODL 7.6, the expand navigation property will be added as a select item (PathSelectItem).
            // After ODL 7.6 (include 7.6), the expand navigation property will not be added.
            // Comment the following codes for visibility later.
            /*
            var parentSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is NavigationPropertySegment));
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)parentSelectItem.SelectedPath.FirstSegment).NavigationProperty.Name);
            */
            Assert.Empty(clause.SelectedItems.OfType<PathSelectItem>().Where(item => item.SelectedPath.FirstSegment is NavigationPropertySegment));

            var expandedItem = Assert.Single(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)expandedItem.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(expandedItem.LevelsOption);

            // Level 2.
            clause = expandedItem.SelectAndExpand;
            Assert.False(clause.AllSelected);
            Assert.Equal(3, clause.SelectedItems.Count());

            var idSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is PropertySegment));
            Assert.Equal("ID", ((PropertySegment)idSelectItem.SelectedPath.FirstSegment).Property.Name);

            var parentSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is NavigationPropertySegment));
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)parentSelectItem.SelectedPath.FirstSegment).NavigationProperty.Name);

            expandedItem = Assert.Single(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)expandedItem.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(expandedItem.LevelsOption);

            clause = expandedItem.SelectAndExpand;
            Assert.False(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            idSelectItem = Assert.IsType<PathSelectItem>(clause.SelectedItems.Single());
            Assert.Equal("ID", ((PropertySegment)idSelectItem.SelectedPath.FirstSegment).Property.Name);
        }

        [Fact]
        public void ProcessLevelsCorrectly_WithMultipleProperties()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity"));
            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=Parent($levels=max),DerivedAncestors($levels=2;$select=ID)),BaseEntities($levels=2)",
                context: context);
            selectExpand.LevelsMaxLiteralExpansionDepth = 3;

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            Assert.True(clause.AllSelected);
            Assert.Equal(2, clause.SelectedItems.Count());

            // Top level Parent.
            var parent = Assert.Single(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "Parent"));
            Assert.Null(parent.LevelsOption);

            var clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Equal(2, clauseOfParent.SelectedItems.Count());

            // Level 1 of inline Parent.
            var inlineParent = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "Parent"));
            Assert.Null(inlineParent.LevelsOption);

            // Level 2 of inline Parent.
            var inlineParentClause = inlineParent.SelectAndExpand;
            Assert.True(inlineParentClause.AllSelected);
            Assert.Single(inlineParentClause.SelectedItems);

            inlineParent = Assert.IsType<ExpandedNavigationSelectItem>(inlineParentClause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)inlineParent.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(inlineParent.LevelsOption);

            inlineParentClause = inlineParent.SelectAndExpand;
            Assert.True(inlineParentClause.AllSelected);
            Assert.Empty(inlineParentClause.SelectedItems);

            // Level 1 of inline DerivedAncestors.
            var inlineDerivedAncestors = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "DerivedAncestors"));
            Assert.Null(inlineDerivedAncestors.LevelsOption);

            // Level 2 of inline DerivedAncestors.
            var inlineDerivedAncestorsClause = inlineDerivedAncestors.SelectAndExpand;
            Assert.False(inlineDerivedAncestorsClause.AllSelected);
            Assert.Equal(3, inlineDerivedAncestorsClause.SelectedItems.Count());

            var idItem = Assert.Single(inlineDerivedAncestorsClause.SelectedItems.OfType<PathSelectItem>().Where(
               item => item.SelectedPath.FirstSegment is PropertySegment));
            Assert.Equal("ID", ((PropertySegment)idItem.SelectedPath.FirstSegment).Property.Name);

            var derivedAncestorsItem = Assert.Single(inlineDerivedAncestorsClause.SelectedItems.OfType<PathSelectItem>().Where(
               item => item.SelectedPath.FirstSegment is NavigationPropertySegment));
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)derivedAncestorsItem.SelectedPath.FirstSegment).NavigationProperty.Name);

            inlineDerivedAncestors = Assert.Single(inlineDerivedAncestorsClause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)inlineDerivedAncestors.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(inlineDerivedAncestors.LevelsOption);

            inlineDerivedAncestorsClause = inlineDerivedAncestors.SelectAndExpand;
            Assert.False(inlineDerivedAncestorsClause.AllSelected);
            Assert.Single(inlineDerivedAncestorsClause.SelectedItems);

            idItem = Assert.Single(inlineDerivedAncestorsClause.SelectedItems.OfType<PathSelectItem>());
            Assert.Equal("ID", ((PropertySegment)idItem.SelectedPath.FirstSegment).Property.Name);

            // Level 1 of BaseEntities.
            var baseEntities = Assert.Single(clause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "BaseEntities"));
            Assert.Null(baseEntities.LevelsOption);

            // Level 2 of BaseEntities.
            var baseEntitiesClause = baseEntities.SelectAndExpand;
            Assert.True(baseEntitiesClause.AllSelected);
            Assert.Single(baseEntitiesClause.SelectedItems);

            baseEntities = Assert.IsType<ExpandedNavigationSelectItem>(baseEntitiesClause.SelectedItems.Single());
            Assert.Equal(
                "BaseEntities",
                ((NavigationPropertySegment)baseEntities.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(baseEntities.LevelsOption);

            baseEntitiesClause = baseEntities.SelectAndExpand;
            Assert.True(baseEntitiesClause.AllSelected);
            Assert.Empty(baseEntitiesClause.SelectedItems);
        }

        [Fact]
        public void ProcessLevelsCorrectly_WithNestedLevels()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity"));
            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=DerivedAncestors($levels=2);$levels=max)",
                context: context);
            selectExpand.LevelsMaxLiteralExpansionDepth = 4;

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            Assert.True(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            // Level 1 of Parent.
            var parent = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)parent.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(parent.LevelsOption);

            var clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Equal(2, clauseOfParent.SelectedItems.Count());

            // Level 1 of DerivedAncestors.
            var derivedAncestors = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "DerivedAncestors"));
            Assert.Null(derivedAncestors.LevelsOption);

            var clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Single(clauseOfDerivedAncestors.SelectedItems);

            // Level 2 of DerivedAncestors.
            derivedAncestors = Assert.IsType<ExpandedNavigationSelectItem>(clauseOfDerivedAncestors.SelectedItems.Single());
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)derivedAncestors.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Empty(clauseOfDerivedAncestors.SelectedItems);

            // Level 2 of Parent.
            parent = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "Parent"));
            Assert.Null(parent.LevelsOption);

            clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Single(clauseOfParent.SelectedItems);

            // Level 1 of DerivedAncestors.
            derivedAncestors = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "DerivedAncestors"));
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Single(clauseOfDerivedAncestors.SelectedItems);

            // Level 2 of DerivedAncestors.
            derivedAncestors = Assert.IsType<ExpandedNavigationSelectItem>(clauseOfDerivedAncestors.SelectedItems.Single());
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)derivedAncestors.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Empty(clauseOfDerivedAncestors.SelectedItems);
        }

        [Fact]
        public void ProcessLevelsCorrectly_WithMaxNestedLevels()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.LevelsEntity"));
            context.RequestContainer = new MockContainer();
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=DerivedAncestors($levels=max);$levels=2)",
                context: context);

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            Assert.True(clause.AllSelected);
            Assert.Single(clause.SelectedItems);

            // Level 1 of Parent.
            var parent = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)parent.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(parent.LevelsOption);

            var clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Equal(2, clauseOfParent.SelectedItems.Count());

            // Level 1 of DerivedAncestors.
            var derivedAncestors = Assert.Single(
                clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                    ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "DerivedAncestors")
                );
            Assert.Null(derivedAncestors.LevelsOption);

            var clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Empty(clauseOfDerivedAncestors.SelectedItems);

            // Level 2 of Parent.
            parent = Assert.Single(
                clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                    ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "Parent")
                );
            Assert.Null(parent.LevelsOption);

            clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Empty(clauseOfParent.SelectedItems);
        }

        [Theory]
        [InlineData("http://test")]
        [InlineData("http://test?$expand=Friend($levels=max)")]
        public void ProcessLevelsCorrectly_WithAutoExpand(string url)
        {
            // Arrange
            var model = GetAutoExpandEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("Microsoft.AspNet.OData.Test.Common.Models.AutoExpandCustomer"));
            var request = RequestFactory.Create(HttpMethod.Get, url);
            ODataQueryOptions queryOption = new ODataQueryOptions(context, request);
            queryOption.AddAutoSelectExpandProperties();
            var selectExpand = queryOption.SelectExpand;

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            Assert.True(clause.AllSelected);
            Assert.Equal(2, clause.SelectedItems.Count());

            // Level 1 of Customer.
            var cutomer = Assert.Single(
                clause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                            ((NavigationPropertySegment) item.PathToNavigationProperty.FirstSegment).NavigationProperty
                                .Name == "Friend")
                );

            var clauseOfCustomer = cutomer.SelectAndExpand;
            Assert.True(clauseOfCustomer.AllSelected);
            Assert.Equal(2, clauseOfCustomer.SelectedItems.Count());

            // Order under Customer.
            var order = Assert.Single(
                clause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                            ((NavigationPropertySegment) item.PathToNavigationProperty.FirstSegment).NavigationProperty
                                .Name == "Order")
                );
            Assert.Null(order.LevelsOption);

            var clauseOfOrder = order.SelectAndExpand;
            Assert.True(clauseOfOrder.AllSelected);
            Assert.Single(clauseOfOrder.SelectedItems);

            // Choice Order under Order
            var choiceOrder = Assert.IsType<ExpandedNavigationSelectItem>(clauseOfOrder.SelectedItems.Single());
            Assert.Null(choiceOrder.LevelsOption);
            Assert.True(choiceOrder.SelectAndExpand.AllSelected);
            Assert.Empty(choiceOrder.SelectAndExpand.SelectedItems);

            // Level 2 of Order.
            order = Assert.Single(
                clauseOfCustomer.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                            ((NavigationPropertySegment) item.PathToNavigationProperty.FirstSegment).NavigationProperty
                                .Name == "Order")
                );
            Assert.Null(order.LevelsOption);

            clauseOfOrder = order.SelectAndExpand;
            Assert.True(clauseOfOrder.AllSelected);
            Assert.Empty(clauseOfOrder.SelectedItems);

            // Level 2 of Customer.
            cutomer = Assert.Single(
                clauseOfCustomer.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                    item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                            ((NavigationPropertySegment) item.PathToNavigationProperty.FirstSegment).NavigationProperty
                                .Name == "Friend")
                );
            Assert.Null(cutomer.LevelsOption);

            clauseOfCustomer = cutomer.SelectAndExpand;
            Assert.True(clauseOfCustomer.AllSelected);
            Assert.Empty(clauseOfCustomer.SelectedItems);
        }

        private IEdmModel GetAutoExpandEdmModel()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<AutoExpandCustomer>("AutoExpandCustomers");
            builder.EntitySet<AutoExpandOrder>("AutoExpandOrders");
            builder.EntitySet<AutoExpandChoiceOrder>("AutoExpandChoiceOrders");
            return builder.GetEdmModel();
        }
    }
}
