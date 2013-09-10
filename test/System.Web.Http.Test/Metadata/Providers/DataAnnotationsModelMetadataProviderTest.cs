// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.Metadata.Providers
{
    public class DataAnnotationsModelMetadataProviderTest
    {
        [Fact]
        public void GetMetadataForPropertiesSetTypesAndPropertyNames()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            IEnumerable<ModelMetadata> result = provider.GetMetadataForProperties("foo", typeof(string));

            // Assert
            Assert.True(result.Any(m => m.ModelType == typeof(int)
                                        && m.PropertyName == "Length"
                                        && (int)m.Model == 3));
        }

        [Fact]
        public void GetMetadataForPropertySetsTypeAndPropertyName()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForProperty(null, typeof(string), "Length");

            // Assert
            Assert.Equal(typeof(int), result.ModelType);
            Assert.Equal("Length", result.PropertyName);
        }

        [Fact]
        public void GetMetadataForTypeSetsTypeWithNullPropertyName()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act
            ModelMetadata result = provider.GetMetadataForType(null, typeof(string));

            // Assert
            Assert.Equal(typeof(string), result.ModelType);
            Assert.Null(result.PropertyName);
        }

        // [ReadOnly] & [Editable] tests

        private class ReadOnlyModel
        {
            public int NoAttributes { get; set; }

            [ReadOnly(true)]
            public int ReadOnlyAttribute { get; set; }

            [Editable(false)]
            public int EditableAttribute { get; set; }

            [ReadOnly(true)]
            [Editable(true)]
            public int BothAttributes { get; set; }

            // Editable trumps ReadOnly
        }

        [Fact]
        public void ReadOnlyTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.False(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "NoAttributes").IsReadOnly);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "ReadOnlyAttribute").IsReadOnly);
            Assert.True(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "EditableAttribute").IsReadOnly);
            Assert.False(provider.GetMetadataForProperty(null, typeof(ReadOnlyModel), "BothAttributes").IsReadOnly);
        }

        // [Display] & [DisplayName] tests

        private class DisplayModel
        {
            public int NoAttribute { get; set; }

            // Description + Name combination.

            [Display]
            public int NothingSet { get; set; }

            [Display(Name = "")]
            public int EmptyDisplayName { get; set; }

            [Display(Description = "Description text1")]
            public int DescriptionSet { get; set; }

            [Display(Name = "Name text1")]
            public int NameSet { get; set; }

            [Display(Description = "Description text2", Name = "Name text2")]
            public int BothSet { get; set; }

            [Display(Name = "String1", ResourceType = typeof(Resources))]
            public int Localized { get; set; }
        }

        [Fact]
        public void DataAnnotationsNameTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.Equal("NoAttribute", provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").GetDisplayName());
            Assert.Equal("NothingSet", provider.GetMetadataForProperty(null, typeof(DisplayModel), "NothingSet").GetDisplayName());
            Assert.Equal("", provider.GetMetadataForProperty(null, typeof(DisplayModel), "EmptyDisplayName").GetDisplayName());
            Assert.Equal("DescriptionSet", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionSet").GetDisplayName());
            Assert.Equal("Name text1", provider.GetMetadataForProperty(null, typeof(DisplayModel), "NameSet").GetDisplayName());
            Assert.Equal("Name text2", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothSet").GetDisplayName());

            Assert.NotEqual("String1", Resources.String1);
            Assert.Equal(Resources.String1, provider.GetMetadataForProperty(null, typeof(DisplayModel), "Localized").GetDisplayName());
        }

        [Fact]
        public void DataAnnotationsDescriptionTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Description);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NothingSet").Description);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NameSet").Description);
            Assert.Equal("Description text1", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionSet").Description);
            Assert.Equal("Description text2", provider.GetMetadataForProperty(null, typeof(DisplayModel), "BothSet").Description);
        }
    }
}
