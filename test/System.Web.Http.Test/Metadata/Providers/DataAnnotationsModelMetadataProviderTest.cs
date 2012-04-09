// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Metadata.Providers
{
    public class DataAnnotationsModelMetadataProviderTest : MarshalByRefObject
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

        class ReadOnlyModel
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

        class DisplayModel
        {
            public int NoAttribute { get; set; }

            // Description

            [Display]
            public int DescriptionNotSet { get; set; }

            [Display(Description = "Description text")]
            public int DescriptionSet { get; set; }
        }

        [Fact]
        public void DescriptionTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            // Act & Assert
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "NoAttribute").Description);
            Assert.Null(provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionNotSet").Description);
            Assert.Equal("Description text", provider.GetMetadataForProperty(null, typeof(DisplayModel), "DescriptionSet").Description);
        }
    }

    [RunWith(typeof(PartialTrustRunner))]
    public class CachedDataAnnotationsModelMetadataProviderPartialTrustTest : DataAnnotationsModelMetadataProviderTest { }
}
