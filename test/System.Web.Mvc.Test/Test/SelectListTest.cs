// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Web.Mvc.Test
{
    public class SelectListTest
    {
        [Fact]
        public void Constructor1SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            SelectList selectList = new SelectList(items);

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Null(selectList.DataValueField);
            Assert.Null(selectList.DataTextField);
            Assert.Null(selectList.SelectedValues);
            Assert.Null(selectList.SelectedValue);
        }

        [Fact]
        public void Constructor2SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            object selectedValue = new object();

            // Act
            SelectList selectList = new SelectList(items, selectedValue);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Null(selectList.DataValueField);
            Assert.Null(selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            Assert.Single(selectedValues);
            Assert.Same(selectedValue, selectedValues[0]);
        }

        [Fact]
        public void Constructor3SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            SelectList selectList = new SelectList(items, "SomeValueField", "SomeTextField");

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("SomeValueField", selectList.DataValueField);
            Assert.Equal("SomeTextField", selectList.DataTextField);
            Assert.Null(selectList.SelectedValues);
            Assert.Null(selectList.SelectedValue);
        }

        [Fact]
        public void Constructor4SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            object selectedValue = new object();

            // Act
            SelectList selectList = new SelectList(items, "SomeValueField", "SomeTextField", selectedValue);
            List<object> selectedValues = selectList.SelectedValues.Cast<object>().ToList();

            // Assert
            Assert.Same(items, selectList.Items);
            Assert.Equal("SomeValueField", selectList.DataValueField);
            Assert.Equal("SomeTextField", selectList.DataTextField);
            Assert.Same(selectedValue, selectList.SelectedValue);
            Assert.Single(selectedValues);
            Assert.Same(selectedValue, selectedValues[0]);
        }
    }
}
