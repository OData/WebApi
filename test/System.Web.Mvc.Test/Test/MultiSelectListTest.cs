// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class MultiSelectListTest
    {
        [Fact]
        public void Constructor1SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Null(multiSelect.DataValueField);
            Assert.Null(multiSelect.DataTextField);
            Assert.Null(multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor2SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, selectedValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Null(multiSelect.DataValueField);
            Assert.Null(multiSelect.DataTextField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor3SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField");

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Null(multiSelect.SelectedValues);
        }

        [Fact]
        public void Constructor4SetsProperties()
        {
            // Arrange
            IEnumerable items = new object[0];
            IEnumerable selectedValues = new object[0];

            // Act
            MultiSelectList multiSelect = new MultiSelectList(items, "SomeValueField", "SomeTextField", selectedValues);

            // Assert
            Assert.Same(items, multiSelect.Items);
            Assert.Equal("SomeValueField", multiSelect.DataValueField);
            Assert.Equal("SomeTextField", multiSelect.DataTextField);
            Assert.Same(selectedValues, multiSelect.SelectedValues);
        }

        [Fact]
        public void ConstructorWithNullItemsThrows()
        {
            Assert.ThrowsArgumentNull(
                delegate { new MultiSelectList(null /* items */, "dataValueField", "dataTextField", null /* selectedValues */); }, "items");
        }

        [Fact]
        public void GetListItemsThrowsOnBindingFailure()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleFieldObjects(),
                                                              "Text", "Value", new string[] { "A", "C", "T" });

            // Assert
            Assert.ThrowsHttpException(
                delegate { IList<SelectListItem> listItems = multiSelect.GetListItems(); }, "DataBinding: 'System.Web.Mvc.Test.MultiSelectListTest+Item' does not contain a property with the name 'Text'.", 500);
        }

        [Fact]
        public void GetListItemsWithoutValueField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleStrings());

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Null(listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.False(listItems[0].Selected);
            Assert.Null(listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Null(listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.False(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithoutValueFieldWithSelections()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleStrings(), new string[] { "Alpha", "Charlie", "Tango" });

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Null(listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.Null(listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Null(listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithValueField()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(), "Letter", "FullWord");

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.False(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.False(listItems[2].Selected);
        }

        [Fact]
        public void GetListItemsWithValueFieldWithSelections()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(),
                                                              "Letter", "FullWord", new string[] { "A", "C", "T" });

            // Act
            IList<SelectListItem> listItems = multiSelect.GetListItems();

            // Assert
            Assert.Equal(3, listItems.Count);
            Assert.Equal("A", listItems[0].Value);
            Assert.Equal("Alpha", listItems[0].Text);
            Assert.True(listItems[0].Selected);
            Assert.Equal("B", listItems[1].Value);
            Assert.Equal("Bravo", listItems[1].Text);
            Assert.False(listItems[1].Selected);
            Assert.Equal("C", listItems[2].Value);
            Assert.Equal("Charlie", listItems[2].Text);
            Assert.True(listItems[2].Selected);
        }

        [Fact]
        public void IEnumerableWithAnonymousObjectsAndTextValueFields()
        {
            // Arrange
            MultiSelectList multiSelect = new MultiSelectList(GetSampleAnonymousObjects(),
                                                              "Letter", "FullWord", new string[] { "A", "C", "T" });

            // Act
            IEnumerator enumerator = multiSelect.GetEnumerator();
            enumerator.MoveNext();
            SelectListItem firstItem = enumerator.Current as SelectListItem;
            SelectListItem lastItem = null;

            while (enumerator.MoveNext())
            {
                lastItem = enumerator.Current as SelectListItem;
            }

            // Assert
            Assert.True(firstItem != null);
            Assert.Equal("Alpha", firstItem.Text);
            Assert.Equal("A", firstItem.Value);
            Assert.True(firstItem.Selected);

            Assert.True(lastItem != null);
            Assert.Equal("Charlie", lastItem.Text);
            Assert.Equal("C", lastItem.Value);
            Assert.True(lastItem.Selected);
        }

        internal static IEnumerable GetSampleAnonymousObjects()
        {
            return new[]
            {
                new { Letter = 'A', FullWord = "Alpha" },
                new { Letter = 'B', FullWord = "Bravo" },
                new { Letter = 'C', FullWord = "Charlie" }
            };
        }

        internal static IEnumerable GetSampleFieldObjects()
        {
            return new[]
            {
                new Item { Text = "A", Value = "Alpha" },
                new Item { Text = "B", Value = "Bravo" },
                new Item { Text = "C", Value = "Charlie" }
            };
        }

        internal static List<SelectListItem> GetSampleListObjects()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            string selectedSSN = "111111111";

            foreach (Person person in GetSamplePeople())
            {
                list.Add(new SelectListItem
                {
                    Text = person.FirstName,
                    Value = person.SSN,
                    Selected = String.Equals(person.SSN, selectedSSN)
                });
            }
            return list;
        }

        internal static IEnumerable<SelectListItem> GetSampleIEnumerableObjects()
        {
            Person[] people = GetSamplePeople();

            string selectedSSN = "111111111";
            IEnumerable<SelectListItem> list = from person in people
                                               select new SelectListItem
                                               {
                                                   Text = person.FirstName,
                                                   Value = person.SSN,
                                                   Selected = String.Equals(person.SSN, selectedSSN)
                                               };
            return list;
        }

        internal static IEnumerable GetSampleStrings()
        {
            return new string[] { "Alpha", "Bravo", "Charlie" };
        }

        internal static Person[] GetSamplePeople()
        {
            return new Person[]
            {
                new Person
                {
                    FirstName = "John",
                    SSN = "123456789"
                },
                new Person
                {
                    FirstName = "Jane",
                    SSN = "987654321"
                },
                new Person
                {
                    FirstName = "Joe",
                    SSN = "111111111"
                }
            };
        }

        internal class Item
        {
            public string Text;
            public string Value;
        }

        internal class Person
        {
            public string FirstName { get; set; }

            public string SSN { get; set; }
        }
    }
}
