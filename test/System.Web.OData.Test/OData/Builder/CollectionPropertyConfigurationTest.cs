// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder
{
    public class CollectionPropertyConfigurationTest
    {
        public static TheoryDataSet<PropertyInfo, Type> GetValidPropertiesAndElementTypes
        {
            get
            {
                Type type = typeof(LotsOfCollectionProperties);

                return new TheoryDataSet<PropertyInfo, Type>
                {
                    { type.GetProperty("ListStrings"), typeof(string) },
                    { type.GetProperty("ListBools"), typeof(bool) },
                    { type.GetProperty("ListGuids"), typeof(Guid) },
                    { type.GetProperty("ListRandomComplexType"), typeof(RandomComplexType) },
                    { type.GetProperty("EnumerableStrings"), typeof(string) },
                    { type.GetProperty("EnumerableBools"), typeof(bool) },
                    { type.GetProperty("EnumerableGuids"), typeof(Guid) },
                    { type.GetProperty("EnumerableRandomComplexType"), typeof(RandomComplexType) },
                    { type.GetProperty("CollectionStrings"), typeof(string) },
                    { type.GetProperty("CollectionBools"), typeof(bool) },
                    { type.GetProperty("CollectionGuids"), typeof(Guid) },
                    { type.GetProperty("CollectionRandomComplexType"), typeof(RandomComplexType) },
                    { type.GetProperty("ArrayStrings"), typeof(string) },
                    { type.GetProperty("ArrayBools"), typeof(bool) },
                    { type.GetProperty("ArrayGuids"), typeof(Guid) },
                    { type.GetProperty("ArrayRandomComplexType"), typeof(RandomComplexType) },
                    { type.GetProperty("RandomListStrings"), typeof(string) },
                    { type.GetProperty("RandomListBools"), typeof(bool) },
                    { type.GetProperty("RandomListGuids"), typeof(Guid) },
                    { type.GetProperty("RandomRandomComplexType"), typeof(RandomComplexType) }
                };
            }
        }

        [Theory]
        [PropertyData("GetValidPropertiesAndElementTypes")]
        public void HasCorrectKindPropertyInfoAndName(PropertyInfo property, Type elementType)
        {
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            CollectionPropertyConfiguration configuration = new CollectionPropertyConfiguration(property, structuralType.Object);
            Assert.Equal(PropertyKind.Collection, configuration.Kind);
            Assert.Equal(elementType, configuration.ElementType);
            Assert.Equal(elementType, configuration.RelatedClrType);
            Assert.Equal(property, configuration.PropertyInfo);
            Assert.Equal(property.Name, configuration.Name);
            Assert.Equal(structuralType.Object, configuration.DeclaringType);
        }

        [Fact]
        public void HasCollectionException()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                PropertyInfo nonCollectionProperty = typeof(LotsOfCollectionProperties).GetProperty("NonCollectionProperty");
                Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
                CollectionPropertyConfiguration configuration = new CollectionPropertyConfiguration(nonCollectionProperty, structuralType.Object);
            });
        }

        [Theory]
        [PropertyData("GetValidPropertiesAndElementTypes")]
        public void CanCorrectlyDetectCollectionProperties(PropertyInfo property, Type elementType)
        {
            Mock<StructuralTypeConfiguration> structuralType = new Mock<StructuralTypeConfiguration>();
            CollectionPropertyConfiguration configuration = new CollectionPropertyConfiguration(property, structuralType.Object);
            Assert.Same(property, configuration.PropertyInfo);
            Assert.Same(elementType, configuration.ElementType);
            Assert.Same(property.Name, configuration.Name);
        }

        internal class LotsOfCollectionProperties
        {
            public List<string> ListStrings { get; set; }
            public List<bool> ListBools { get; set; }
            public List<Guid> ListGuids { get; set; }
            public List<RandomComplexType> ListRandomComplexType { get; set; }
            public IEnumerable<string> EnumerableStrings { get; set; }
            public IEnumerable<bool> EnumerableBools { get; set; }
            public IEnumerable<Guid> EnumerableGuids { get; set; }
            public IEnumerable<RandomComplexType> EnumerableRandomComplexType { get; set; }
            public ICollection<string> CollectionStrings { get; set; }
            public ICollection<bool> CollectionBools { get; set; }
            public ICollection<Guid> CollectionGuids { get; set; }
            public ICollection<RandomComplexType> CollectionRandomComplexType { get; set; }
            public string[] ArrayStrings { get; set; }
            public bool[] ArrayBools { get; set; }
            public Guid[] ArrayGuids { get; set; }
            public RandomComplexType[] ArrayRandomComplexType { get; set; }
            public RandomList<string> RandomListStrings { get; set; }
            public RandomList<bool> RandomListBools { get; set; }
            public RandomList<Guid> RandomListGuids { get; set; }
            public RandomList<RandomComplexType> RandomRandomComplexType { get; set; }
            public string NonCollectionProperty { get; set; }
        }

        internal class RandomComplexType { }

        internal class RandomList<T> : List<T> { }
    }
}
