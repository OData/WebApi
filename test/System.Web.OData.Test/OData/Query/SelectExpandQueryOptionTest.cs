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
                () => new SelectExpandQueryOption("select", "expand", context, null),
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
    }
}
