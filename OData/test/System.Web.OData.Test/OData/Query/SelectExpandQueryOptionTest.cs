// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Query
{
    public class SelectExpandQueryOptionTest
    {
        private CustomersModelWithInheritance _model = new CustomersModelWithInheritance();

        [Fact]
        public void Ctor_ThrowsArgumentNull_Context()
        {
            Assert.ThrowsArgumentNull(
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
            Assert.Throws<ArgumentException>(
                () => new SelectExpandQueryOption(select: null, expand: null, context: context),
                "'select' and 'expand' cannot be both null or empty.");
        }

        [Fact]
        public void Ctor_ThrowsArgument_IfContextIsNotForAnEntityType()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(int));

            // Act & Assert
            Assert.ThrowsArgument(
                () => new SelectExpandQueryOption(select: "Name", expand: "Name", context: context),
                "context",
                "The type 'Edm.Int32' is not an entity type. Only entity types support $select and $expand.");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_QueryOptionParser()
        {
            // Arrange
            ODataQueryContext context = new ODataQueryContext(_model.Model, typeof(int));

            // Act & Assert
            Assert.ThrowsArgumentNull(
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
            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(_model.Customers));
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer), odataPath);
            SelectExpandQueryOption option = new SelectExpandQueryOption("ID,Name,SimpleEnum,Orders", "Orders", context);

            // Act
            SelectExpandClause selectExpandClause = option.SelectExpandClause;

            // Assert
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<PathSelectItem>());
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
        }

        [Fact]
        public void SelectExpandClause_Property_ParsesWithEdmTypeAndNavigationSource()
        {
            // Arrange
            IEdmModel model = _model.Model;
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(Customer)));
            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(_model.Customers));
            ODataQueryContext context = new ODataQueryContext(model, _model.Customer, odataPath);
            SelectExpandQueryOption option = new SelectExpandQueryOption("ID,Name,SimpleEnum,Orders", "Orders", context);

            // Act
            SelectExpandClause selectExpandClause = option.SelectExpandClause;

            // Assert
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<PathSelectItem>());
            Assert.NotEmpty(selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>());
        }

        [Theory]
        [InlineData("ID", null)]
        [InlineData("LastName,FirstName", null)]
        [InlineData("LastName,    FirstName", null)]
        [InlineData("LastName,FirstName", "Orders")]
        [InlineData("LastName,FirstName,Orders", "Orders")]
        [InlineData("LastName,FirstName,Orders", "Orders")]
        [InlineData("Orders", "Orders,Orders($expand=Customer),Orders($expand=Customer($expand=Orders))")]
        [InlineData("SimpleEnum", "Orders")]
        public void SelectExpandClause_CanParse_ModelBuiltForQueryable(string select, string expand)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver());
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(new HttpConfiguration(), isQueryCompositionMode: true);
            builder.EntityType<Customer>();
            IEdmModel model = builder.GetEdmModel();

            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));
            SelectExpandQueryOption option = new SelectExpandQueryOption(select, expand, context);

            // Act & Assert
            Assert.DoesNotThrow(() => option.SelectExpandClause.ToString());
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
            SelectExpandQueryOption option = new SelectExpandQueryOption(select, expand, context);

            // Act
            Assert.Throws<ODataException>(
                () => option.SelectExpandClause.ToString(),
                error);
        }

        [Fact]
        public void Property_SelectExpandClause_WorksWithUnTypedContext()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
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
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: context);
            IQueryable queryable = new Mock<IQueryable>().Object;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => selectExpand.ApplyTo(queryable, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Fact]
        public void ApplyTo_OnSingleEntity_WithUnTypedContext_Throws_InvalidOperation()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataQueryContext context = new ODataQueryContext(model.Model, model.Customer);
            SelectExpandQueryOption selectExpand = new SelectExpandQueryOption(select: "ID", expand: null, context: context);
            object entity = new object();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => selectExpand.ApplyTo(entity, new ODataQuerySettings()),
                "The query option is not bound to any CLR type. 'ApplyTo' is only supported with a query option bound to a CLR type.");
        }

        [Fact]
        public void ProcessLevelsCorrectly_AllSelected()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("System.Web.OData.Routing.LevelsEntity"));
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=Parent($levels=2))",
                context: context);

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            // Level 1.
            Assert.True(clause.AllSelected);
            Assert.Equal(1, clause.SelectedItems.Count());

            var item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            // Level 2.
            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Equal(1, clause.SelectedItems.Count());

            item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            // Level 3.
            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Equal(1, clause.SelectedItems.Count());

            item = Assert.IsType<ExpandedNavigationSelectItem>(clause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(item.LevelsOption);

            clause = item.SelectAndExpand;
            Assert.True(clause.AllSelected);
            Assert.Equal(0, clause.SelectedItems.Count());
        }

        [Fact]
        public void ProcessLevelsCorrectly_NotAllSelected()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("System.Web.OData.Routing.LevelsEntity"));
            var selectExpand = new SelectExpandQueryOption(
                select: "Name",
                expand: "Parent($select=ID;$levels=max)",
                context: context);

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            // Level 1.
            Assert.False(clause.AllSelected);
            Assert.Equal(3, clause.SelectedItems.Count());

            var nameSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is PropertySegment));
            Assert.Equal("Name", ((PropertySegment)nameSelectItem.SelectedPath.FirstSegment).Property.Name);

            var parentSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
                item => item.SelectedPath.FirstSegment is NavigationPropertySegment));
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)parentSelectItem.SelectedPath.FirstSegment).NavigationProperty.Name);

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

            parentSelectItem = Assert.Single(clause.SelectedItems.OfType<PathSelectItem>().Where(
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
            Assert.Equal(1, clause.SelectedItems.Count());

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
                model.FindDeclaredType("System.Web.OData.Routing.LevelsEntity"));
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
            Assert.Equal(1, inlineParentClause.SelectedItems.Count());

            inlineParent = Assert.IsType<ExpandedNavigationSelectItem>(inlineParentClause.SelectedItems.Single());
            Assert.Equal(
                "Parent",
                ((NavigationPropertySegment)inlineParent.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(inlineParent.LevelsOption);

            inlineParentClause = inlineParent.SelectAndExpand;
            Assert.True(inlineParentClause.AllSelected);
            Assert.Equal(0, inlineParentClause.SelectedItems.Count());

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
            Assert.Equal(1, inlineDerivedAncestorsClause.SelectedItems.Count());

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
            Assert.Equal(1, baseEntitiesClause.SelectedItems.Count());

            baseEntities = Assert.IsType<ExpandedNavigationSelectItem>(baseEntitiesClause.SelectedItems.Single());
            Assert.Equal(
                "BaseEntities",
                ((NavigationPropertySegment)baseEntities.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(baseEntities.LevelsOption);

            baseEntitiesClause = baseEntities.SelectAndExpand;
            Assert.True(baseEntitiesClause.AllSelected);
            Assert.Equal(0, baseEntitiesClause.SelectedItems.Count());
        }

        [Fact]
        public void ProcessLevelsCorrectly_WithNestedLevels()
        {
            // Arrange
            var model = ODataLevelsTest.GetEdmModel();
            var context = new ODataQueryContext(
                model,
                model.FindDeclaredType("System.Web.OData.Routing.LevelsEntity"));
            var selectExpand = new SelectExpandQueryOption(
                select: null,
                expand: "Parent($expand=DerivedAncestors($levels=2);$levels=max)",
                context: context);

            // Act
            SelectExpandClause clause = selectExpand.ProcessLevels();

            // Assert
            Assert.True(clause.AllSelected);
            Assert.Equal(1, clause.SelectedItems.Count());

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
            Assert.Equal(1, clauseOfDerivedAncestors.SelectedItems.Count());

            // Level 2 of DerivedAncestors.
            derivedAncestors = Assert.IsType<ExpandedNavigationSelectItem>(clauseOfDerivedAncestors.SelectedItems.Single());
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)derivedAncestors.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Equal(0, clauseOfDerivedAncestors.SelectedItems.Count());

            // Level 2 of Parent.
            parent = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "Parent"));
            Assert.Null(parent.LevelsOption);

            clauseOfParent = parent.SelectAndExpand;
            Assert.True(clauseOfParent.AllSelected);
            Assert.Equal(1, clauseOfParent.SelectedItems.Count());

            // Level 1 of DerivedAncestors.
            derivedAncestors = Assert.Single(clauseOfParent.SelectedItems.OfType<ExpandedNavigationSelectItem>().Where(
                item => item.PathToNavigationProperty.FirstSegment is NavigationPropertySegment &&
                ((NavigationPropertySegment)item.PathToNavigationProperty.FirstSegment).NavigationProperty.Name == "DerivedAncestors"));
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Equal(1, clauseOfDerivedAncestors.SelectedItems.Count());

            // Level 2 of DerivedAncestors.
            derivedAncestors = Assert.IsType<ExpandedNavigationSelectItem>(clauseOfDerivedAncestors.SelectedItems.Single());
            Assert.Equal(
                "DerivedAncestors",
                ((NavigationPropertySegment)derivedAncestors.PathToNavigationProperty.FirstSegment).NavigationProperty.Name);
            Assert.Null(derivedAncestors.LevelsOption);

            clauseOfDerivedAncestors = derivedAncestors.SelectAndExpand;
            Assert.True(clauseOfDerivedAncestors.AllSelected);
            Assert.Equal(0, clauseOfDerivedAncestors.SelectedItems.Count());
        }
    }
}
