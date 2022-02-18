//-----------------------------------------------------------------------------
// <copyright file="TypeHelperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class TypeHelperTest
    {
        /// <summary>
        /// Collection types to test.
        /// </summary>
        public static TheoryDataSet<Type, Type> CollectionTypesData
        {
            get
            {
                return new TheoryDataSet<Type, Type>
                {
                    { typeof(ICollection<string>), typeof(string) },
                    { typeof(IList<string>), typeof(string) },
                    { typeof(List<int>), typeof(int) },
                    { typeof(CustomBoolCollection), typeof(bool) },
                    { typeof(IEnumerable<int>), typeof(int) },
                    { typeof(int[]), typeof(int) },
                    { typeof(CustomIntCollection), typeof(int) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CollectionTypesData))]
        public void IsCollection_with_Collections(Type collectionType, Type elementType)
        {
            Type type;
            Assert.True(TypeHelper.IsCollection(collectionType, out type));
            Assert.Equal(elementType, type);
            Assert.True(TypeHelper.IsCollection(collectionType));
        }

        [Theory]
        [MemberData(nameof(CollectionTypesData))]
        public void GetInnerElementType(Type collectionType, Type elementType)
        {
            Assert.Equal(elementType, TypeHelper.GetInnerElementType(collectionType));
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(string))]
        public void IsCollection_with_NonCollections(Type type)
        {
            Assert.False(TypeHelper.IsCollection(type));
        }

        [Theory]
        [InlineData(typeof(int), typeof(int?))]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(DateTime), typeof(DateTime?))]
        [InlineData(typeof(int?), typeof(int?))]
        [InlineData(typeof(IEnumerable), typeof(IEnumerable))]
        [InlineData(typeof(int[]), typeof(int[]))]
        [InlineData(typeof(string[]), typeof(string[]))]
        public void ToNullable_Returns_ExpectedValue(Type type, Type expectedResult)
        {
            Assert.Equal(expectedResult, TypeHelper.ToNullable(type));
        }

        [Theory]
        [InlineData(typeof(CustomAbstractClass), true)]
        [InlineData(typeof(CustomConcreteClass), false)]
        [InlineData(typeof(string), false)]
        public void IsAbstract(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsAbstract(type));
        }

        [Theory]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(long), false)]
        [InlineData(typeof(CustomBoolCollection), true)]
        [InlineData(typeof(string), true)]
        public void IsClass(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsClass(type));
        }

        [Theory]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(List<string>), true)]
        [InlineData(typeof(CustomBoolCollection), false)]
        public void IsGenericType(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsGenericType(type));
        }

        [Theory]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(List<string>), false)]
        [InlineData(typeof(CustomBoolCollection), false)]
        [InlineData(typeof(List<>), true)]
        public void IsGenericTypeDefinition(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsGenericTypeDefinition(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(ICollection), true)]
        [InlineData(typeof(IEnumerable), true)]
        [InlineData(typeof(string), false)]
        public void IsInterface(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsInterface(type));
        }

        [Theory]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(int?), true)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(bool?), true)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(CustomBoolCollection), true)]
        public void IsNullable(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsNullable(type));
        }

        [Theory]
        [InlineData(typeof(object), true)]
        [InlineData(typeof(TypeHelperTest), true)]
        [InlineData(typeof(CustomInternalClass), false)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(CustomBoolCollection), false)]
        public void IsPublic(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsPublic(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(string), false)]
        public void IsPrimitive(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsPrimitive(type));
        }

        [Theory]
        [InlineData(typeof(object), typeof(object), true)]
        [InlineData(typeof(int), typeof(object), false)]
        [InlineData(typeof(long), typeof(int), false)]
        [InlineData(typeof(string), typeof(object), false)]
        [InlineData(typeof(object), typeof(string), true)]
        [InlineData(typeof(CustomBoolCollection), typeof(List<bool>), false)]
        [InlineData(typeof(CustomBoolCollection), typeof(List<int>), false)]
        [InlineData(typeof(CustomAbstractClass), typeof(CustomConcreteClass), true)]
        public void IsTypeAssignableFrom(Type type,Type fromType, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsTypeAssignableFrom(type, fromType));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(string), false)]
        public void IsValueType(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsValueType(type));
        }

        [Theory]
        [InlineData(typeof(object), true)]
        [InlineData(typeof(TypeHelperTest), true)]
        [InlineData(typeof(CustomInternalClass), false)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(CustomBoolCollection), false)]
        public void IsVisible(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsVisible(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(ICollection), false)]
        [InlineData(typeof(MemberTypes), true)]
        [InlineData(typeof(string), false)]
        public void IsEnum(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsEnum(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(ICollection), false)]
        [InlineData(typeof(DateTime), true)]
        [InlineData(typeof(string), false)]
        public void IsDateTime(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsDateTime(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(ICollection), false)]
        [InlineData(typeof(TimeSpan), true)]
        [InlineData(typeof(string), false)]
        public void IsTimeSpan(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsTimeSpan(type));
        }

        [Theory]
        [InlineData(typeof(IEnumerable), false)]
        [InlineData(typeof(IQueryable), true)]
        [InlineData(typeof(IEnumerable<CustomInternalClass>), false)]
        [InlineData(typeof(IQueryable<CustomInternalClass>), true)]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(List<CustomInternalClass>), false)]
        [InlineData(typeof(CustomInternalClass[]), false)]
        public void IsIQueryable(Type type, bool isIQueryable)
        {
            Assert.Equal(isIQueryable, TypeHelper.IsIQueryable(type));
        }

        [Theory]
        [InlineData(typeof(object), false)]
        [InlineData(typeof(ICollection), false)]
        [InlineData(typeof(IEnumerable), false)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(DateTime), true)]
        [InlineData(typeof(Decimal), true)]
        [InlineData(typeof(Guid), true)]
        [InlineData(typeof(DateTimeOffset), true)]
        [InlineData(typeof(TimeSpan), true)]
        public void IsQueryPrimitiveType(Type type, bool expected)
        {
            Assert.Equal(expected, TypeHelper.IsQueryPrimitiveType(type));
        }


        [Theory]
        [InlineData(typeof(object), typeof(object))]
        [InlineData(typeof(int), typeof(int))]
        [InlineData(typeof(int?), typeof(int))]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(CustomBoolCollection), typeof(CustomBoolCollection))]
        public void GetInnerMostElementType(Type collectionType, Type elementType)
        {
            Assert.Equal(elementType, TypeHelper.GetInnerMostElementType(collectionType));
        }

        [Theory]
        [InlineData(typeof(object), null)]
        [InlineData(typeof(int), null)]
        [InlineData(typeof(int[]), typeof(int))]
        [InlineData(typeof(CustomBoolCollection), typeof(bool))]
        [InlineData(typeof(IQueryable<int>), typeof(int))]
        [InlineData(typeof(IEnumerable<bool>), typeof(bool))]
        [InlineData(typeof(string), typeof(char))]
        [InlineData(typeof(Task<int>), null)]
        [InlineData(typeof(Task<string>), typeof(char))]
        [InlineData(typeof(Task<IEnumerable<bool>>), typeof(bool))]
        [InlineData(typeof(IEnumerable<IEnumerable<bool>>), typeof(IEnumerable<bool>))]
        public void GetImplementedIEnumerableType(Type collectionType, Type elementType)
        {
            Assert.Equal(elementType, TypeHelper.GetImplementedIEnumerableType(collectionType));
        }

        [Fact]
        public void GetLoadedTypes()
        {
            // Arrange
            MockType baseType =
                 new MockType("BaseType")
                 .Property(typeof(int), "ID");

            MockType derivedType =
                new MockType("DerivedType")
                .Property(typeof(int), "DerivedTypeId")
                .BaseType(baseType);

            MockAssembly assembly = new MockAssembly(baseType, derivedType);
            IWebApiAssembliesResolver resolver = WebApiAssembliesResolverFactory.Create(assembly);
            IEnumerable<Type> foundTypes = TypeHelper.GetLoadedTypes(resolver);

            IEnumerable<string> definedNames = assembly.GetTypes().Select(t => t.FullName);
            IEnumerable<string> foundNames = foundTypes.Select(t => t.FullName);

            foreach (string name in definedNames)
            {
                Assert.Contains(name, foundNames);
            }

            Assert.DoesNotContain(typeof(TypeHelperTest), foundTypes);
        }

        [Fact]
        public void GetPropertyDeclaredOnSelf()
        {
            // Arrange & Act
            var prop = TypeHelper.GetProperty(typeof(TypeA), "Prop");

            // Assert
            Assert.Equal("Prop", prop.Name);
            Assert.Equal(typeof(int), prop.PropertyType);
            Assert.Equal(typeof(TypeA), prop.DeclaringType);
        }

        [Fact]
        public void GetPropertyRedeclaredOnSelf()
        {
            // Arrange & Act
            var prop = TypeHelper.GetProperty(typeof(TypeB), "Prop");

            // Assert
            Assert.Equal("Prop", prop.Name);
            Assert.Equal(typeof(string), prop.PropertyType);
            Assert.Equal(typeof(TypeB), prop.DeclaringType);
        }

        [Fact]
        public void GetPropertyDeclaredOnImmediateParent()
        {
            // Arrange & Act
            var prop = TypeHelper.GetProperty(typeof(TypeC), "Prop");

            // Assert
            Assert.Equal("Prop", prop.Name);
            Assert.Equal(typeof(int), prop.PropertyType);
            Assert.Equal(typeof(TypeA), prop.DeclaringType);
        }

        [Fact]
        public void GetPropertyRedeclaredOnImmediateParent()
        {
            // Arrange & Act
            var prop = TypeHelper.GetProperty(typeof(TypeD), "Prop");

            // Assert
            Assert.Equal("Prop", prop.Name);
            Assert.Equal(typeof(string), prop.PropertyType);
            Assert.Equal(typeof(TypeB), prop.DeclaringType);
        }

        [Fact]
        public void GetPropertyDeclaredOnNonImmediateParent()
        {
            // Arrange & Act
            var prop = TypeHelper.GetProperty(typeof(TypeE), "Prop");

            // Assert
            Assert.Equal("Prop", prop.Name);
            Assert.Equal(typeof(string), prop.PropertyType);
            Assert.Equal(typeof(TypeB), prop.DeclaringType);
        }

        [Fact]
        public void GetPropertyForNonUndeclaredProperty()
        {
            // Arrange & Act
            var undeclared = TypeHelper.GetProperty(typeof(TypeA), "Undeclared");

            // Assert
            Assert.Null(undeclared);
        }

        /// <summary>
        /// Custom internal class
        /// </summary>
        internal class CustomInternalClass
        {
        }

        /// <summary>
        /// Custom collection of bool
        /// </summary>
        private sealed class CustomBoolCollection : List<bool>
        {
        }

        /// <summary>
        /// Custom collection of int
        /// </summary>
        private class CustomIntCollection : List<int>
        {
        }

        /// <summary>
        /// Custom abstract class
        /// </summary>
        private abstract class CustomAbstractClass
        {
            public abstract int Area();
        }

        /// <summary>
        /// Custom abstract class
        /// </summary>
        private class CustomConcreteClass : CustomAbstractClass
        {
            public override int Area() { return 42; }
        }

        private class TypeA
        {
            public int Prop { get; set; }
        }

        private class TypeB : TypeA
        {
            public new string Prop { get; set; }
        }

        private class TypeC : TypeA
        {
        }

        private class TypeD : TypeB
        {
        }

        private class TypeE : TypeD
        {
        }
    }
}
