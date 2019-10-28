// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class SelectExpandNodeTest
    {
        private CustomersModelWithInheritance _model = new CustomersModelWithInheritance();

        [Fact]
        public void Ctor_ThrowsArgumentNull_StructuredType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SelectExpandNode(selectExpandClause: null, structuredType: null, model: EdmCoreModel.Instance),
                "structuredType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmModel()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new SelectExpandNode(selectExpandClause: null, structuredType: new Mock<IEdmEntityType>().Object, model: null),
                "model");
        }

        [Theory]
        [InlineData("ID,ID", "ID")]
        [InlineData("NS.upgrade,NS.upgrade", "NS.upgrade")]
        public void DuplicatedSelectPathInOneDollarSelectThrows(string select, string error)
        {
            // Arrange
            ODataQueryOptionParser parser = new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string> { { "$select", select } });

            // Act
            Action test = () => parser.ParseSelectAndExpand();

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                String.Format("Found mutliple select terms with same select path '{0}' at one $select, please combine them together.", error));
        }

        [Theory]
        [InlineData(null, null, false, "City,ID,Name,SimpleEnum", "Account,Address,OtherAccounts")] // no select and expand -> select all
        [InlineData(null, null, true, "City,ID,Name,SimpleEnum,SpecialCustomerProperty", "Account,Address,OtherAccounts,SpecialAddress")] // no select and expand on derived type -> select all
        [InlineData("ID", null, false, "ID", null)] // simple select -> select requested
        [InlineData("ID", null, true, "ID", null)] // simple select on derived type -> select requested
        [InlineData("*", null, false, "City,ID,Name,SimpleEnum", "Account,Address,OtherAccounts")] // simple select with wild card -> select all, no duplication
        [InlineData("*", null, true, "City,ID,Name,SimpleEnum,SpecialCustomerProperty", "Account,Address,OtherAccounts,SpecialAddress")] // simple select with wild card on derived type -> select all, no duplication
        [InlineData("ID,*", null, false, "City,ID,Name,SimpleEnum", "Account,Address,OtherAccounts")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,*", null, true, "City,ID,Name,SimpleEnum,SpecialCustomerProperty", "Account,Address,OtherAccounts,SpecialAddress")] // simple select with wild card and duplicate -> select all, no duplicates
        [InlineData("ID,Name", null, false, "ID,Name", null)] // multiple select -> select requested
        [InlineData("ID,Name", null, true, "ID,Name", null)] // multiple select on derived type -> select requested
        [InlineData("Orders", "Orders", false, null, null)] // only expand -> select no structural property
        [InlineData("Orders", "Orders", true, null, null)] // only expand -> select no structural property
        [InlineData(null, "Orders", false, "City,ID,Name,SimpleEnum", "Account,Address,OtherAccounts")] // simple expand -> select all
        [InlineData(null, "Orders", true, "City,ID,Name,SimpleEnum,SpecialCustomerProperty", "Account,Address,OtherAccounts,SpecialAddress")] // simple expand on derived type -> select all
        [InlineData("ID,Name,Orders", "Orders", false, "ID,Name", null)] // expand and select -> select requested
        [InlineData("ID,Name,Orders", "Orders", true, "ID,Name", null)] // expand and select on derived type -> select requested
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", null, false, null, null)] // select derived type properties -> select none
        [InlineData("NS.SpecialCustomer/SpecialCustomerProperty", null, true, "SpecialCustomerProperty", null)] // select derived type properties on derived type -> select requested
        [InlineData("ID", "Orders($select=ID),Orders($expand=Customer($select=ID))", true, "ID", null)] // deep expand and selects
        public void SelectProperties_SelectsExpectedProperties_OnCustomer(
            string select, string expand, bool specialCustomer, string structuralsToSelect, string complexesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);

            IEdmStructuredType structuralType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, structuralType, _model.Model);

            // Assert
            if (structuralsToSelect == null)
            {
                Assert.Null(selectExpandNode.SelectedStructuralProperties);
            }
            else
            {
                Assert.Equal(structuralsToSelect, String.Join(",", selectExpandNode.SelectedStructuralProperties.Select(p => p.Name).OrderBy(n => n)));
            }

            if (complexesToSelect == null)
            {
                Assert.Null(selectExpandNode.SelectedComplexProperties);
            }
            else
            {
                Assert.Equal(complexesToSelect, String.Join(",", selectExpandNode.SelectedComplexProperties.Select(p => p.Name).OrderBy(n => n)));
            }
        }

        [Theory]
        [InlineData("ID,Name,Orders", "Orders", false, "Amount,City,ID")] // expand and select -> select all
        [InlineData("ID,Name,Orders", "Orders", true, "Amount,City,ID,SpecialOrderProperty")] // expand and select on derived type -> select all
        [InlineData("ID,Name,Orders", "Orders($select=ID)", false, "ID")] // expand and select properties on expand -> select requested
        [InlineData("ID,Name,Orders", "Orders($select=ID)", true, "ID")] // expand and select properties on expand on derived type -> select requested
        [InlineData("Orders", "Orders,Orders($expand=Customer)", false, "Amount,City,ID")]
        [InlineData("Orders", "Orders,Orders($expand=Customer)", true, "Amount,City,ID,SpecialOrderProperty")]
        public void SelectProperties_Selects_ExpectedProperties_OnExpandedOrders(string select, string expand, bool specialOrder, string structuralPropertiesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);
            SelectExpandClause nestedSelectExpandClause = selectExpandClause.SelectedItems.OfType<ExpandedNavigationSelectItem>().Single().SelectAndExpand;
            IEdmStructuredType structuralType = specialOrder ? _model.SpecialOrder : _model.Order;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(nestedSelectExpandClause, structuralType, _model.Model);

            // Assert
            Assert.Equal(structuralPropertiesToSelect, String.Join(",", selectExpandNode.SelectedStructuralProperties.Select(p => p.Name).OrderBy(n => n)));
        }

        [Theory]
        [InlineData("Address/Street,Address/City,Address/ZipCode", "Address", "Street,City,ZipCode")] // complex
        [InlineData("Address($select=Street,City,ZipCode)", "Address", "Street,City,ZipCode")]
        [InlineData("OtherAccounts/Bank,OtherAccounts/CardNum", "OtherAccounts", "Bank,CardNum")] // Collection complex
        [InlineData("OtherAccounts($select=Bank,CardNum)", "OtherAccounts", "Bank,CardNum")]
        public void SelectProperties_OnSubPrimitivePropertyFromComplex_SelectsExpectedProperties(string select, string firstSelected, string secondSelected)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null);

            // Act: Top Level
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert
            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal(firstSelected, firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Act: Sub Level
            IEdmStructuredType subLevelElementType = firstLevelSelected.Key.Type.ToStructuredType();
            SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, subLevelElementType, _model.Model);
            Assert.Null(subSelectExpandNode.SelectedComplexes);

            // Assert
            Assert.NotNull(subSelectExpandNode.SelectedStructuralProperties);
            var selectedProperties = secondSelected.Split(',');
            Assert.Equal(selectedProperties.Length, subSelectExpandNode.SelectedStructuralProperties.Count);
            Assert.Equal(secondSelected, String.Join(",", subSelectExpandNode.SelectedStructuralProperties.Select(s => s.Name)));
        }

        [Theory]
        [InlineData("Account/BankAddress/Street,Account/BankAddress/City,Account/BankAddress/ZipCode")]
        [InlineData("Account/BankAddress($select=Street,City,ZipCode)")]
        public void SelectProperties_OnMultipleLevelsPropertyFromComplex_SelectsExpectedProperties(string select)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null);

            // Act: First Level
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal("Account", firstLevelSelected.Key.Name);
            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Act: Second Level
            SelectExpandNode secondSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Account, _model.Model);

            // Assert
            Assert.Null(secondSelectExpandNode.SelectedStructuralProperties);
            Assert.NotNull(secondSelectExpandNode.SelectedComplexes);
            var secondLevelSelected = Assert.Single(secondSelectExpandNode.SelectedComplexes);
            Assert.Equal("BankAddress", secondLevelSelected.Key.Name);
            Assert.NotNull(secondLevelSelected.Value);
            Assert.NotNull(secondLevelSelected.Value.SelectAndExpand);

            // Act: Third Level
            SelectExpandNode thirdSelectExpandNode = new SelectExpandNode(secondLevelSelected.Value.SelectAndExpand, _model.Address, _model.Model);

            // Assert
            Assert.Null(thirdSelectExpandNode.SelectedComplexes);
            Assert.NotNull(thirdSelectExpandNode.SelectedStructuralProperties);
            Assert.Equal(3, thirdSelectExpandNode.SelectedStructuralProperties.Count);
            Assert.Equal(new[] { "Street", "City", "ZipCode" }, thirdSelectExpandNode.SelectedStructuralProperties.Select(s => s.Name));
        }

        [Theory]
        [InlineData("Account/DynamicProperty", "Account")]
        [InlineData("Account($select=DynamicProperty)", "Account")]
        [InlineData("OtherAccounts/DynamicProperty", "OtherAccounts")]
        [InlineData("OtherAccounts($select=DynamicProperty)", "OtherAccounts")]
        public void SelectProperties_OnSubDynamicFromComplex_SelectsExpectedProperties(string select, string firstSelected)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null);

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert: Top Level
            Assert.False(selectExpandNode.SelectAllDynamicProperties);
            Assert.Null(selectExpandNode.SelectedDynamicProperties);
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.Null(selectExpandNode.SelectedNavigationProperties);
            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal(firstSelected, firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Assert: Second Level
            SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Account, _model.Model);
            Assert.Null(subSelectExpandNode.SelectedComplexes);
            Assert.Null(subSelectExpandNode.SelectedStructuralProperties);
            Assert.Null(subSelectExpandNode.SelectedNavigationProperties);
            Assert.False(subSelectExpandNode.SelectAllDynamicProperties);
            Assert.NotNull(subSelectExpandNode.SelectedDynamicProperties);
            Assert.Equal("DynamicProperty", Assert.Single(subSelectExpandNode.SelectedDynamicProperties));
        }

        [Theory]
        [InlineData("Account/AccountOrderNav", "Account")]
        [InlineData("Account($select=AccountOrderNav)", "Account")]
        [InlineData("OtherAccounts/AccountOrderNav", "OtherAccounts")]
        [InlineData("OtherAccounts($select=AccountOrderNav)", "OtherAccounts")]
        public void SelectProperties_OnSubNavigationPropertyFromComplex_SelectsExpectedProperties(string select, string firstSelected)
        {
            // Arrange
            _model.Account.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "AccountOrderNav",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = _model.Order
            });

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null);

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert: Top Level
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.Null(selectExpandNode.SelectedNavigationProperties);
            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal(firstSelected, firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Assert: Second Level
            SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Account, _model.Model);
            Assert.Null(subSelectExpandNode.SelectedComplexes);
            Assert.Null(subSelectExpandNode.SelectedStructuralProperties);
            Assert.NotNull(subSelectExpandNode.SelectedNavigationProperties);
            Assert.Equal("AccountOrderNav", Assert.Single(subSelectExpandNode.SelectedNavigationProperties).Name);
        }

        [Fact]
        public void SelectProperties_OnMultipleSubPropertiesFromComplex_SelectsExpectedProperties()
        {
            // Arrange
            string select = "Account,Account/Bank,Account/BankAddress/Street";

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, null);

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert: Top Level
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.Null(selectExpandNode.SelectedNavigationProperties);
            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal("Account", firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);
            Assert.True(firstLevelSelected.Value.SelectAndExpand.AllSelected);

            // Assert: Second Level
            SelectExpandNode secondSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Account, _model.Model);
            Assert.NotNull(secondSelectExpandNode.SelectedStructuralProperties);
            Assert.Equal(2, secondSelectExpandNode.SelectedStructuralProperties.Count); // Because it's select all from first select item.
            Assert.Equal(new[] { "Bank", "CardNum" }, secondSelectExpandNode.SelectedStructuralProperties.Select(s => s.Name));

            Assert.Null(secondSelectExpandNode.SelectedNavigationProperties);

            Assert.NotNull(secondSelectExpandNode.SelectedComplexes);
            var secondLevelSelected = Assert.Single(secondSelectExpandNode.SelectedComplexes);
            Assert.Equal("BankAddress", secondLevelSelected.Key.Name);
            Assert.NotNull(secondLevelSelected.Value);
            Assert.NotNull(secondLevelSelected.Value.SelectAndExpand);
            Assert.False(secondLevelSelected.Value.SelectAndExpand.AllSelected);

            // Assert: Third level
            SelectExpandNode thirdSelectExpandNode = new SelectExpandNode(secondLevelSelected.Value.SelectAndExpand, _model.Address, _model.Model);
            Assert.NotNull(thirdSelectExpandNode.SelectedStructuralProperties);
            Assert.Equal("Street", Assert.Single(thirdSelectExpandNode.SelectedStructuralProperties).Name);
            Assert.Null(thirdSelectExpandNode.SelectedNavigationProperties);
            Assert.Null(thirdSelectExpandNode.SelectedComplexes);
        }

        [Theory]
        [InlineData("$select=Account/Bank&$expand=Account/AccountOrderNav", "Account")]
        [InlineData("$select=Account($select=Bank)&$expand=Account/AccountOrderNav", "Account")]
        [InlineData("$select=OtherAccounts/Bank&$expand=OtherAccounts/AccountOrderNav", "OtherAccounts")]
        [InlineData("$select=OtherAccounts($select=Bank)&$expand=OtherAccounts/AccountOrderNav", "OtherAccounts")]
        public void SelectProperties_OnSubSelectAndExpandFromComplex_SelectsExpectedProperties(string selectExpand, string firstSelected)
        {
            // Arrange
            string[] items = selectExpand.Split('&');
            Assert.Equal(2, items.Length);

            string select = items[0].Substring(8);
            string expand = items[1].Substring(8);

            _model.Account.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "AccountOrderNav",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = _model.Order
            });

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert: Top Level
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.Null(selectExpandNode.SelectedNavigationProperties);
            Assert.Null(selectExpandNode.ExpandedProperties); // Not expanded at first level

            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal(firstSelected, firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Assert: Second Level
            SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Account, _model.Model);
            Assert.Null(subSelectExpandNode.SelectedComplexes);
            Assert.NotNull(subSelectExpandNode.SelectedStructuralProperties);
            Assert.Equal("Bank", Assert.Single(subSelectExpandNode.SelectedStructuralProperties).Name);

            Assert.NotNull(subSelectExpandNode.ExpandedProperties);
            var expandedProperty = Assert.Single(subSelectExpandNode.ExpandedProperties);
            Assert.Equal("AccountOrderNav", expandedProperty.Key.Name);

            Assert.NotNull(expandedProperty.Value);
            Assert.NotNull(expandedProperty.Value.SelectAndExpand);
            Assert.True(expandedProperty.Value.SelectAndExpand.AllSelected);
            Assert.Empty(expandedProperty.Value.SelectAndExpand.SelectedItems);
        }

        [Fact]
        public void SelectProperties_OnSubPropertyWithTypeCastFromComplex_SelectsExpectedProperties()
        {
            // Arrange
            EdmComplexType subComplexType = new EdmComplexType("NS", "CnAddress", _model.Address);
            subComplexType.AddStructuralProperty("SubAddressProperty", EdmPrimitiveTypeKind.String);
            subComplexType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "CnAddressOrderNav",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = _model.Order
            });
            _model.Model.AddElement(subComplexType);

            string select = "Address/NS.CnAddress/SubAddressProperty";
            string expand = "Address/NS.CnAddress/CnAddressOrderNav";

            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, _model.Customer, _model.Model);

            // Assert: Top Level
            Assert.Null(selectExpandNode.SelectedStructuralProperties);
            Assert.Null(selectExpandNode.SelectedNavigationProperties);
            Assert.Null(selectExpandNode.ExpandedProperties); // Not expanded at first level

            Assert.NotNull(selectExpandNode.SelectedComplexes);
            var firstLevelSelected = Assert.Single(selectExpandNode.SelectedComplexes);
            Assert.Equal("Address", firstLevelSelected.Key.Name);

            Assert.NotNull(firstLevelSelected.Value);
            Assert.NotNull(firstLevelSelected.Value.SelectAndExpand);

            // Assert: Second Level
            {
                // use the base type to test
                SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, _model.Address, _model.Model);
                Assert.Null(subSelectExpandNode.SelectedStructuralProperties);
                Assert.Null(subSelectExpandNode.SelectedComplexes);
                Assert.Null(subSelectExpandNode.ExpandedProperties);
                Assert.Null(subSelectExpandNode.SelectedNavigationProperties);
            }
            {
                // use the sub type to test
                SelectExpandNode subSelectExpandNode = new SelectExpandNode(firstLevelSelected.Value.SelectAndExpand, subComplexType, _model.Model);
                Assert.Null(subSelectExpandNode.SelectedComplexes);
                Assert.NotNull(subSelectExpandNode.SelectedStructuralProperties);
                Assert.Equal("SubAddressProperty", Assert.Single(subSelectExpandNode.SelectedStructuralProperties).Name);

                Assert.NotNull(subSelectExpandNode.ExpandedProperties);
                var expandedProperty = Assert.Single(subSelectExpandNode.ExpandedProperties);
                Assert.Equal("CnAddressOrderNav", expandedProperty.Key.Name);

                Assert.NotNull(expandedProperty.Value);
                Assert.NotNull(expandedProperty.Value.SelectAndExpand);
                Assert.True(expandedProperty.Value.SelectAndExpand.AllSelected);
                Assert.Empty(expandedProperty.Value.SelectAndExpand.SelectedItems);
            }
        }

        [Theory]
        [InlineData(null, null, false, "Orders")] // no select and expand -> select all
        [InlineData(null, null, true, "Orders,SpecialOrders")] // no select and expand on derived type -> select all
        [InlineData("ID", null, false, null)] // simple select -> select none 
        [InlineData("ID", null, true, null)] // simple select on derived type -> select none
        [InlineData(null, "Orders", false, null)] // simple expand -> select non expanded
        [InlineData(null, "Orders", true, "SpecialOrders")] // simple expand on derived type -> select non expanded
        [InlineData("ID", "Orders", false, null)] // simple expand without corresponding select -> select none
        [InlineData("ID", "Orders", true, null)] // simple expand without corresponding select on derived type -> select none
        [InlineData("ID,Orders", "Orders", false, null)] // simple expand with corresponding select -> select none
        [InlineData("ID,Orders", "Orders", true, null)] // simple expand with corresponding select on derived type -> select none
        [InlineData("ID,Orders", null, false, "Orders")] // simple select without corresponding expand -> select requested
        [InlineData("ID,Orders", null, true, "Orders")] // simple select with corresponding expand on derived type -> select requested
        [InlineData("NS.SpecialCustomer/SpecialOrders", null, false, null)] // select derived type properties -> select none
        [InlineData("NS.SpecialCustomer/SpecialOrders", null, true, "SpecialOrders")] // select derived type properties on derived type -> select requested
        public void SelectNavigationProperties_SelectsExpectedProperties(string select, string expand, bool specialCustomer, string propertiesToSelect)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);
            IEdmStructuredType structuralType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, structuralType, _model.Model);

            // Assert
            if (propertiesToSelect == null)
            {
                Assert.Null(selectExpandNode.SelectedNavigationProperties);
            }
            else
            {
                Assert.Equal(propertiesToSelect, String.Join(",", selectExpandNode.SelectedNavigationProperties.Select(p => p.Name)));
            }
        }

        [Theory]
        [InlineData(null, null, false, null)] // no select and expand -> expand none
        [InlineData(null, null, true, null)] // no select and expand on derived type -> expand none
        [InlineData("Orders", null, false, null)] // simple select and no expand -> expand none
        [InlineData("Orders", null, true, null)] // simple select and no expand on derived type -> expand none
        [InlineData(null, "Orders", false, "Orders")] // simple expand and no select -> expand requested
        [InlineData(null, "Orders", true, "Orders")] // simple expand and no select on derived type -> expand requested
        [InlineData(null, "Orders,Orders,Orders", false, "Orders")] // duplicate expand -> expand requested
        [InlineData(null, "Orders,Orders,Orders", true, "Orders")] // duplicate expand on derived type -> expand requested
        [InlineData("ID", "Orders", false, "Orders")] // Expanded navigation properties MUST be returned, even if they are not specified as a selectItem.
        [InlineData("ID", "Orders", true, "Orders")] // Expanded navigation properties MUST be returned, even if they are not specified as a selectItem.
        [InlineData("Orders", "Orders", false, "Orders")] // only expand -> expand requested
        [InlineData("ID,Orders", "Orders", false, "Orders")] // simple expand and expand in select -> expand requested
        [InlineData("ID,Orders", "Orders", true, "Orders")] // simple expand and expand in select on derived type -> expand requested
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", false, null)] // expand derived navigation property -> expand requested
        [InlineData(null, "NS.SpecialCustomer/SpecialOrders", true, "SpecialOrders")] // expand derived navigation property on derived type -> expand requested
        public void ExpandNavigationProperties_ExpandsExpectedProperties(string select, string expand, bool specialCustomer, string propertiesToExpand)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand);
            IEdmStructuredType structuralType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, structuralType, _model.Model);

            // Assert
            if (propertiesToExpand == null)
            {
                Assert.Null(selectExpandNode.ExpandedProperties);
            }
            else
            {
                Assert.Equal(propertiesToExpand, String.Join(",", selectExpandNode.ExpandedProperties.Select(p => p.Key.Name)));
            }
        }

        [Theory]
        [InlineData(null, false, 1, 8)] // no select and no expand means to select all operations
        [InlineData(null, true, 2, 10)]
        [InlineData("*", false, null, null)] // select * means to select no operations
        [InlineData("*", true, null, null)]
        [InlineData("NS.*", false, 1, 8)] // select wild card actions means to select all starting with "NS"
        [InlineData("NS.*", true, 2, 10)]
        [InlineData("NS.upgrade", false, 1, null)] // select single action -> select requested action
        [InlineData("NS.upgrade", true, 1, null)]
        [InlineData("NS.SpecialCustomer/NS.specialUpgrade", false, null, null)] // select single derived action on base type -> select nothing
        [InlineData("NS.SpecialCustomer/NS.specialUpgrade", true, 1, null)] // select single derived action on derived type  -> select requested action
        [InlineData("NS.GetSalary", false, null, 1)] // select single function -> select requested function
        [InlineData("NS.GetSalary", true, null, 1)]
        [InlineData("NS.SpecialCustomer/NS.IsSpecialUpgraded", false, null, null)] // select single derived function on base type -> select nothing
        [InlineData("NS.SpecialCustomer/NS.IsSpecialUpgraded", true, null, 1)] // select single derived function on derived type  -> select requested function
        public void OperationsToBeSelected_Selects_ExpectedOperations(string select, bool specialCustomer, int? actionsSelected, int? functionsSelected)
        {
            // Arrange
            SelectExpandClause selectExpandClause = ParseSelectExpand(select, expand: null);
            IEdmStructuredType structuralType = specialCustomer ? _model.SpecialCustomer : _model.Customer;

            // Act
            SelectExpandNode selectExpandNode = new SelectExpandNode(selectExpandClause, structuralType, _model.Model);

            // Assert: Actions
            if (actionsSelected == null)
            {
                Assert.Null(selectExpandNode.SelectedActions);
            }
            else
            {
                Assert.Equal(actionsSelected, selectExpandNode.SelectedActions.Count);
            }

            // Assert: Functions
            if (functionsSelected == null)
            {
                Assert.Null(selectExpandNode.SelectedFunctions);
            }
            else
            {
                Assert.Equal(functionsSelected, selectExpandNode.SelectedFunctions.Count);
            }
        }

        [Fact]
        public void BuildSelectExpandNode_ThrowsODataException_IfUnknownSelectItemPresent()
        {
            // Arrange
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[] { new Mock<SelectItem>().Object }, allSelected: false);
            IEdmStructuredType structuralType = _model.Customer;

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new SelectExpandNode(selectExpandClause, structuralType, _model.Model),
                "$select does not support selections of type 'SelectItemProxy'.");
        }

        #region Test IsComplexOrCollectionComplex
        [Fact]
        public void IsComplexOrCollectionComplex_TestNullInputCorrect()
        {
            // Arrange & Act
            IEdmStructuralProperty primitiveProperty = null;

            // Assert
            Assert.False(SelectExpandNode.IsComplexOrCollectionComplex(primitiveProperty));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsComplexOrCollectionComplex_TestPrimitiveStructuralPropertyCorrect(bool isCollection)
        {
            // Arrange & Act
            var stringType = EdmCoreModel.Instance.GetString(false);
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty primitiveProperty;
            if (isCollection)
            {
                primitiveProperty = entityType.AddStructuralProperty("Codes", new EdmCollectionTypeReference(new EdmCollectionType(stringType)));
            }
            else
            {
                primitiveProperty = entityType.AddStructuralProperty("Id", stringType);
            }

            // Assert
            Assert.False(SelectExpandNode.IsComplexOrCollectionComplex(primitiveProperty));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsComplexOrCollectionComplex_TestComplexStructuralPropertyCorrect(bool isCollection)
        {
            // Arrange & Act
            var complexType = new EdmComplexTypeReference(new EdmComplexType("NS", "Complex"), false);
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");

            IEdmStructuralProperty complexProperty;
            if (isCollection)
            {
                complexProperty = entityType.AddStructuralProperty("Complexes", new EdmCollectionTypeReference(new EdmCollectionType(complexType)));
            }
            else
            {
                complexProperty = entityType.AddStructuralProperty("Single", complexType);
            }

            // Assert
            Assert.True(SelectExpandNode.IsComplexOrCollectionComplex(complexProperty));
        }
        #endregion

        #region Test EdmStructuralTypeInfo
        [Theory]
        [InlineData("Customer", 7, 1, 1, 8)]
        [InlineData("SpecialCustomer", 9, 2, 2, 10)]
        [InlineData("Order", 3, 1, 0, 0)]
        [InlineData("Address", 5, 0, 0, 0)]
        public void EdmStructuralTypeInfoCtor_ReturnsCorrectProperties(string typeName, int structurals, int navigations, int actions, int functions)
        {
            // Assert
            IEdmStructuredType structuralType = _model.Model.SchemaElements.OfType<IEdmSchemaType>().FirstOrDefault(c => c.Name == typeName) as IEdmStructuredType;
            Assert.NotNull(structuralType); // Guard

            // Act
            var structuralTypeInfo = new SelectExpandNode.EdmStructuralTypeInfo(_model.Model, structuralType);

            // Assert
            Assert.NotNull(structuralTypeInfo.AllStructuralProperties);
            Assert.Equal(structurals, structuralTypeInfo.AllStructuralProperties.Count);

            if (navigations == 0)
            {
                Assert.Null(structuralTypeInfo.AllNavigationProperties);
            }
            else
            {
                Assert.NotNull(structuralTypeInfo.AllNavigationProperties);
                Assert.Equal(navigations, structuralTypeInfo.AllNavigationProperties.Count);
            }

            if (actions == 0)
            {
                Assert.Null(structuralTypeInfo.AllActions);
            }
            else
            {
                Assert.NotNull(structuralTypeInfo.AllActions);
                Assert.Equal(actions, structuralTypeInfo.AllActions.Count);
            }

            if (functions == 0)
            {
                Assert.Null(structuralTypeInfo.AllFunctions);
            }
            else
            {
                Assert.NotNull(structuralTypeInfo.AllFunctions);
                Assert.Equal(functions, structuralTypeInfo.AllFunctions.Count);
            }
        }

        [Fact]
        public void EdmStructuralTypeInfo_IsStructuralPropertyDefined_ReturnsCorrectBoolean()
        {
            // Assert
            var structuralTypeInfo = new SelectExpandNode.EdmStructuralTypeInfo(_model.Model, _model.Customer);

            IEdmStructuralProperty property = _model.Customer.DeclaredStructuralProperties().FirstOrDefault();
            IEdmStructuralProperty addressProperty = _model.Address.DeclaredStructuralProperties().FirstOrDefault();

            // Act & Assert
            Assert.True(structuralTypeInfo.IsStructuralPropertyDefined(property));
            Assert.False(structuralTypeInfo.IsStructuralPropertyDefined(addressProperty));
        }

        [Fact]
        public void EdmStructuralTypeInfo_IsNavigationPropertyDefined_ReturnsCorrectBoolean()
        {
            // Assert
            var structuralTypeInfo = new SelectExpandNode.EdmStructuralTypeInfo(_model.Model, _model.Customer);

            IEdmNavigationProperty property = _model.Customer.DeclaredNavigationProperties().FirstOrDefault();
            IEdmNavigationProperty orderProperty = _model.Order.DeclaredNavigationProperties().FirstOrDefault();

            // Act & Assert
            Assert.True(structuralTypeInfo.IsNavigationPropertyDefined(property));
            Assert.False(structuralTypeInfo.IsNavigationPropertyDefined(orderProperty));
        }
        #endregion

        public SelectExpandClause ParseSelectExpand(string select, string expand)
        {
            return new ODataQueryOptionParser(_model.Model, _model.Customer, _model.Customers,
                new Dictionary<string, string>
                {
                    { "$expand", expand == null ? "" : expand },
                    { "$select", select == null ? "" : select }
                }).ParseSelectAndExpand();
        }
    }
}
