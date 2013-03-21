// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.TestCommon;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SemanticAst;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class SelectExpandNodeTest
    {
        private CustomersModelWithInheritance _model = new CustomersModelWithInheritance();
        private Expansion _emptyExpansion = new Expansion(new ExpandItem[0]);

        [Fact]
        public void BuildSelectExpandNode_ThrowsArgumentNull_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => SelectExpandNode.BuildSelectExpandNode(selectExpandClause: null, entityType: null, model: EdmCoreModel.Instance),
                "entityType");
        }

        [Fact]
        public void BuildSelectExpandNode_ThrowsArgumentNull_Model()
        {
            Assert.ThrowsArgumentNull(
                () => SelectExpandNode.BuildSelectExpandNode(
                    selectExpandClause: null,
                    entityType: new Mock<IEdmEntityTypeReference>().Object,
                    model: null),
                "model");
        }

        [Theory]
        [InlineData(null, null, false, "Address,ID,Name")] // no select and expand -> select all
        [InlineData(null, null, true, "Address,ID,Name,SpecialCustomerProperty")] // no select and expand on derived type -> select all
        [InlineData("ID", null, false, "ID")] // simple select -> select requested
        [InlineData("ID", null, true, "ID")] // simple select on derived type -> select requested
        [InlineData("*", null, false, "Address,ID,Name")] // simple select with wild card -> select all, no duplication
        [InlineData("*", null, true, "Address,ID,Name,SpecialCustomerProperty")] // simple select with wild card on derived type -> select all, no duplication
        [InlineData("ID,ID", null, false, "ID")] // simple select with duplicates -> select requested no duplicates
        [InlineData("ID,*", null, false, "Address,ID,Name")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,*", null, true, "Address,ID,Name,SpecialCustomerProperty")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,Name", null, false, "ID,Name")] // multiple select -> select requested
        [InlineData("ID,Name", null, true, "ID,Name")] // multiple select on derived type -> select requested
        [InlineData(null, "Orders", false, "Address,ID,Name")] // simple expand -> select all
        [InlineData(null, "Orders", true, "Address,ID,Name,SpecialCustomerProperty")] // simple expand on derived type -> select all
        [InlineData("ID,Name,Orders", "Orders", false, "ID,Name")] // expand and select -> select requested
        [InlineData("ID,Name,Orders", "Orders", true, "ID,Name")] // expand and select on derived type -> select requested
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", "", false, "")] // select derived type properties -> select none
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", "", true, "SpecialCustomerProperty")] // select derived type properties on derived type -> select requested
        [InlineData("ID,Orders/ID,Orders/Customer/ID", "Orders,Orders/Customer", true, "ID")] // deep expand and selects
        public void GetPropertiesToBeSelected_Selects_ExpectedProperties_OnCustomer(
            string select, string expand, bool specialCustomer, string structuralPropertiesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause =
                ODataUriParser.ParseSelectAndExpand(select, expand, _model.Model, _model.Customer, _model.Customers);
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference((specialCustomer ? _model.SpecialCustomer : _model.Customer), false);

            // Act
            SelectExpandNode selectExpandNode = SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedStructuralProperties;

            // Assert
            Assert.Equal(structuralPropertiesToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData("ID,Name,Orders", "Orders", false, "Amount,ID")] // expand and select -> select all
        [InlineData("ID,Name,Orders", "Orders", true, "Amount,ID,SpecialOrderProperty")] // expand and select on derived type -> select all
        [InlineData("ID,Name,Orders/ID", "Orders", false, "ID")] // expand and select properties on expand -> select requested
        [InlineData("ID,Name,Orders/ID", "Orders", true, "ID")] // expand and select properties on expand on derived type -> select requested
        public void GetPropertiesToBeSelected_Selects_ExpectedProperties_OnExpandedOrders(
            string select, string expand, bool specialOrder, string structuralPropertiesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause =
                ODataUriParser.ParseSelectAndExpand(select, expand, _model.Model, _model.Customer, _model.Customers);
            SelectExpandClause nestedSelectExpandClause = selectExpandClause.Expansion.ExpandItems.SingleOrDefault().SelectExpandOption;

            IEdmEntityTypeReference entityType = new EdmEntityTypeReference((specialOrder ? _model.SpecialOrder : _model.Order), false);

            // Act
            SelectExpandNode selectExpandNode = SelectExpandNode.BuildSelectExpandNode(nestedSelectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedStructuralProperties;

            // Assert
            Assert.Equal(structuralPropertiesToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData(null, null, false, "Orders")] // no select and expand -> select all
        [InlineData(null, null, true, "Orders,SpecialOrders")] // no select and expand on derived type -> select all
        [InlineData("ID", null, false, "")] // simple select -> select none 
        [InlineData("ID", null, true, "")] // simple select on derived type -> select none
        [InlineData(null, "Orders", false, "")] // simple expand -> select non expanded
        [InlineData(null, "Orders", true, "SpecialOrders")] // simple expand on derived type -> select non expanded
        [InlineData("ID", "Orders", false, "")] // simple expand without corresponding select -> select none
        [InlineData("ID", "Orders", true, "")] // simple expand without corresponding select on derived type -> select none
        [InlineData("ID,Orders", "Orders", false, "")] // simple expand with corresponding select -> select none
        [InlineData("ID,Orders", "Orders", true, "")] // simple expand with corresponding select on derived type -> select none
        [InlineData("ID,Orders", null, false, "Orders")] // simple select without corresponding expand -> select requested
        [InlineData("ID,Orders", null, true, "Orders")] // simple select with corresponding expand on derived type -> select requested
        [InlineData("NS.SpecialCustomer/SpecialOrders", "", false, "")] // select derived type properties -> select none
        [InlineData("NS.SpecialCustomer/SpecialOrders", "", true, "SpecialOrders")] // select derived type properties on derived type -> select requested
        public void GetNavigationPropertiesToBeSelected_Selects_ExpectedProperties(
            string select, string expand, bool specialCustomer, string navigationPropertiesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause =
                ODataUriParser.ParseSelectAndExpand(select, expand, _model.Model, _model.Customer, _model.Customers);

            IEdmEntityTypeReference entityType = new EdmEntityTypeReference((specialCustomer ? _model.SpecialCustomer : _model.Customer), false);

            // Act
            SelectExpandNode selectExpandNode = SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedNavigationProperties;

            // Assert
            Assert.Equal(navigationPropertiesToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData(null, null, false, "")] // no select and expand -> expand none
        [InlineData(null, null, true, "")] // no select and expand on derived type -> expand none
        [InlineData("Orders", null, false, "")] // simple select and no expand -> expand none
        [InlineData("Orders", null, true, "")] // simple select and no expand on derived type -> expand none
        [InlineData(null, "Orders", false, "Orders")] // simple expand and no select -> expand requested
        [InlineData(null, "Orders", true, "Orders")] // simple expand and no select on derived type -> expand requested
        [InlineData(null, "Orders,Orders,Orders", false, "Orders")] // duplicate expand -> expand requested
        [InlineData(null, "Orders,Orders,Orders", true, "Orders")] // duplicate expand on derived type -> expand requested
        [InlineData("ID", "Orders", false, "")] // simple expand and expand not in select -> expand none
        [InlineData("ID", "Orders", true, "")] // simple expand and expand not in select on derived type -> expand none
        [InlineData("ID,Orders", "Orders", false, "Orders")] // simple expand and expand in select -> expand requested
        [InlineData("ID,Orders", "Orders", true, "Orders")] // simple expand and expand in select on derived type -> expand requested
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", false, "")] // expand derived navigation property -> expand non
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", true, "SpecialOrders")] // expand derived navigation property on derived type -> expand requested
        public void GetNavigationPropertiesToBeExpanded_Expands_ExpectedProperties(
            string select, string expand, bool specialCustomer, string navigationPropertiesToExpand)
        {
            // Arrange
            SelectExpandClause selectExpandClause =
                ODataUriParser.ParseSelectAndExpand(select, expand, _model.Model, _model.Customer, _model.Customers);

            IEdmEntityTypeReference entityType = new EdmEntityTypeReference((specialCustomer ? _model.SpecialCustomer : _model.Customer), false);

            // Act
            SelectExpandNode selectExpandNode = SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.ExpandedNavigationProperties.Keys;

            // Assert
            Assert.Equal(navigationPropertiesToExpand, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData(null, null, "upgrade")] // no select and no expand -> select all actions
        [InlineData("*", null, "")] // select * -> select no actions
        //[InlineData("ModelWithInheritance.Upgrade", null, "upgrade")] // select single actions -> select requested action
        //[InlineData("ModelWithInheritance.Upgrade,ModelWithInheritance.Upgrade", null, "upgrade")] // select duplicate actions -> de-duplicate
        //[InlineData("ModelWithInheritance.*", null, "upgrade")] // select wild card actions -> select all
        public void GetActionsToBeSelected_Selects_ExpectedActions(
            string select, string expand, string actionsToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause =
                ODataUriParser.ParseSelectAndExpand(select, expand, _model.Model, _model.Customer, _model.Customers);
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(_model.Customer, false);

            // Act
            SelectExpandNode selectExpandNode = SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedActions;

            // Assert
            Assert.Equal(actionsToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Fact]
        public void BuildSelectExpandNode_ThrowsODataException_IfUnknownSelectionIsPresent()
        {
            Selection unknownSelection = new Mock<Selection>().Object;
            SelectExpandClause selectExpandClause = new SelectExpandClause(unknownSelection, _emptyExpansion);
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(_model.Customer, false);

            Assert.Throws<ODataException>(
                () => SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model),
                "$select does not support selections of type 'SelectionProxy'.");
        }

        [Fact]
        public void BuildSelectExpandNode_ThrowsODataException_IfUnknownSelectionItemPresent()
        {
            Selection selection = new PartialSelection(new[] { new Mock<SelectionItem>().Object });
            SelectExpandClause selectExpandClause = new SelectExpandClause(selection, _emptyExpansion);
            IEdmEntityTypeReference entityType = new EdmEntityTypeReference(_model.Customer, false);

            Assert.Throws<ODataException>(
                () => SelectExpandNode.BuildSelectExpandNode(selectExpandClause, entityType, _model.Model),
                "$select does not support selections of type 'SelectionItemProxy'.");
        }

        [Fact]
        public void ValidatePathIsSupported_ThrowsForUnsupportedPath()
        {
            ODataPath path = new ODataPath(ValueSegment.Instance);

            Assert.Throws<ODataException>(
                () => SelectExpandNode.ValidatePathIsSupported(path),
                "A path within the select or expand query option is not supported.");
        }
    }
}
