// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class SelectExpandNodeTest
    {
        private CustomersModelWithInheritance _model = new CustomersModelWithInheritance();

        [Fact]
        public void Ctor_ThrowsArgumentNull_EntityType()
        {
            Assert.ThrowsArgumentNull(
                () => new SelectExpandNode(selectExpandClause: null, entityType: null, model: EdmCoreModel.Instance),
                "entityType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Model()
        {
            Assert.ThrowsArgumentNull(
                () => new SelectExpandNode(
                    selectExpandClause: null,
                    entityType: new Mock<IEdmEntityType>().Object,
                    model: null),
                "model");
        }

        [Theory]
        [InlineData(null, null, false, "Address,City,ID,Name,SimpleEnum")] // no select and expand -> select all
        [InlineData(null, null, true, "Address,City,ID,Name,SimpleEnum,SpecialAddress,SpecialCustomerProperty")] // no select and expand on derived type -> select all
        [InlineData("ID", null, false, "ID")] // simple select -> select requested
        [InlineData("ID", null, true, "ID")] // simple select on derived type -> select requested
        [InlineData("*", null, false, "Address,City,ID,Name,SimpleEnum")] // simple select with wild card -> select all, no duplication
        [InlineData("*", null, true, "Address,City,ID,Name,SimpleEnum,SpecialAddress,SpecialCustomerProperty")] // simple select with wild card on derived type -> select all, no duplication
        [InlineData("ID,ID", null, false, "ID")] // simple select with duplicates -> select requested no duplicates
        [InlineData("ID,*", null, false, "Address,City,ID,Name,SimpleEnum")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,*", null, true, "Address,City,ID,Name,SimpleEnum,SpecialAddress,SpecialCustomerProperty")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,Name", null, false, "ID,Name")] // multiple select -> select requested
        [InlineData("ID,Name", null, true, "ID,Name")] // multiple select on derived type -> select requested
        [InlineData("Orders", "Orders", false, "")] // only expand -> select no structural property
        [InlineData("Orders", "Orders", true, "")] // only expand -> select no structural property
        [InlineData(null, "Orders", false, "Address,City,ID,Name,SimpleEnum")] // simple expand -> select all
        [InlineData(null, "Orders", true, "Address,City,ID,Name,SimpleEnum,SpecialAddress,SpecialCustomerProperty")] // simple expand on derived type -> select all
        [InlineData("ID,Name,Orders", "Orders", false, "ID,Name")] // expand and select -> select requested
        [InlineData("ID,Name,Orders", "Orders", true, "ID,Name")] // expand and select on derived type -> select requested
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", "", false, "")] // select derived type properties -> select none
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", "", true, "SpecialCustomerProperty")] // select derived type properties on derived type -> select requested
        [InlineData("ID", "Orders($select=ID),Orders($expand=Customer($select=ID))", true, "ID")] // deep expand and selects
        public void GetPropertiesToBeSelected_Selects_ExpectedProperties_OnCustomer(
            string select, string expand, bool specialCustomer, string structuralPropertiesToSelect)
        {
            // Arrange
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });

            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();
            IEdmEntityType entityType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedStructuralProperties;

            // Assert
            Assert.Equal(structuralPropertiesToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData("ID,Name,Orders", "Orders", false, "Amount,City,ID")] // expand and select -> select all
        [InlineData("ID,Name,Orders", "Orders", true, "Amount,City,ID,SpecialOrderProperty")] // expand and select on derived type -> select all
        [InlineData("ID,Name,Orders", "Orders($select=ID)", false, "ID")] // expand and select properties on expand -> select requested
        [InlineData("ID,Name,Orders", "Orders($select=ID)", true, "ID")] // expand and select properties on expand on derived type -> select requested
        [InlineData("Orders", "Orders,Orders($expand=Customer)", false, "Amount,City,ID")]
        [InlineData("Orders", "Orders,Orders($expand=Customer)", true, "Amount,City,ID,SpecialOrderProperty")]
        public void GetPropertiesToBeSelected_Selects_ExpectedProperties_OnExpandedOrders(
            string select, string expand, bool specialOrder, string structuralPropertiesToSelect)
        {
            // Arrange
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            SelectExpandClause nestedSelectExpandClause = selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;

            IEdmEntityType entityType = specialOrder ? _model.SpecialOrder : _model.Order;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(nestedSelectExpandClause, entityType, _model.Model);
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
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            IEdmEntityType entityType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, entityType, _model.Model);
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
        [InlineData("ID", "Orders", false, "Orders")] // Expanded navigation properties MUST be returned, even if they are not specified as a selectItem.
        [InlineData("ID", "Orders", true, "Orders")] // Expanded navigation properties MUST be returned, even if they are not specified as a selectItem.
        [InlineData("Orders", "Orders", false, "Orders")] // only expand -> expand requested
        [InlineData("ID,Orders", "Orders", false, "Orders")] // simple expand and expand in select -> expand requested
        [InlineData("ID,Orders", "Orders", true, "Orders")] // simple expand and expand in select on derived type -> expand requested
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", false, "")] // expand derived navigation property -> expand non
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", true, "SpecialOrders")] // expand derived navigation property on derived type -> expand requested
        public void GetNavigationPropertiesToBeExpanded_Expands_ExpectedProperties(
            string select, string expand, bool specialCustomer, string navigationPropertiesToExpand)
        {
            // Arrange
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            IEdmEntityType entityType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.ExpandedNavigationProperties.Keys;

            // Assert
            Assert.Equal(navigationPropertiesToExpand, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData(null, null, false, "upgrade")] // no select and no expand -> select all actions
        [InlineData(null, null, true, "specialUpgrade,upgrade")] // no select and no expand -> select all actions
        [InlineData("*", null, false, "")] // select * -> select no actions
        [InlineData("*", null, true, "")] // select * -> select no actions
        [InlineData("NS.upgrade", null, false, "upgrade")] // select single actions -> select requested action
        [InlineData("NS.upgrade", null, true, "upgrade")] // select single actions -> select requested action
        [InlineData("NS.SpecialCustomer/NS.specialUpgrade", null, false, "")] // select single derived action on base type -> select nothing
        [InlineData("NS.SpecialCustomer/NS.specialUpgrade", null, true, "specialUpgrade")] // select single derived action on derived type  -> select requested action
        [InlineData("NS.upgrade,NS.upgrade", null, false, "upgrade")] // select duplicate actions -> de-duplicate
        [InlineData("NS.*", null, false, "upgrade")] // select wild card actions -> select all
        [InlineData("NS.*", null, true, "specialUpgrade,upgrade")] // select wild card actions -> select all
        public void GetActionsToBeSelected_Selects_ExpectedActions(
            string select, string expand, bool specialCustomer, string actionsToSelect)
        {
            // Arrange
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });
            SelectExpandClause selectExpandClause = parser.ParseSelectAndExpand();

            IEdmEntityType entityType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, entityType, _model.Model);
            var result = selectExpandNode.SelectedActions;

            // Assert
            Assert.Equal(actionsToSelect, String.Join(",", result.Select(p => p.Name).OrderBy(n => n)));
        }

        [Fact]
        public void BuildSelectExpandNode_ThrowsODataException_IfUnknownSelectItemPresent()
        {
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[] { new Mock<SelectItem>().Object }, allSelected: false);
            IEdmEntityType entityType = _model.Customer;

            Assert.Throws<ODataException>(
                () => new SelectExpandNode(selectExpandClause, entityType, _model.Model),
                "$select does not support selections of type 'SelectItemProxy'.");
        }

        [Fact]
        public void ValidatePathIsSupported_ThrowsForUnsupportedPath()
        {
            ODataPath path = new ODataPath(new ValueSegment(previousType: null));

            Assert.Throws<ODataException>(
                () => SelectExpandNode.ValidatePathIsSupported(path),
                "A path within the select or expand query option is not supported.");
        }
    }
}
