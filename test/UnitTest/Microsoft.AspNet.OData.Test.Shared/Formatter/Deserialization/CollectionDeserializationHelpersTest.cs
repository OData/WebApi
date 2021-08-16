//-----------------------------------------------------------------------------
// <copyright file="CollectionDeserializationHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
    public class CollectionDeserializationHelpersTest
    {
        public static TheoryDataSet<IList, IEnumerable> CopyItemsToCollectionData
        {
            get
            {
                IList source = new List<int> { 1, 2, 3 };
                return new TheoryDataSet<IList, IEnumerable>
                {
                    { source, new List<int>() },
                    { source, new Collection<int>() },
                    { source, new CustomCollectionWithAdd<int>() },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CopyItemsToCollectionData))]
        public void CopyItemsToCollection(IList oldCollection, IEnumerable newCollection)
        {
            oldCollection.AddToCollection(newCollection, typeof(int), typeof(CollectionDeserializationHelpersTest), "PropertyName", newCollection.GetType());

            Assert.Equal(
                new[] { 1, 2, 3 },
                newCollection as IEnumerable<int>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertToNonStandardEdm()
        {
            IList source = new List<SimpleEnum> { SimpleEnum.First, SimpleEnum.Second, SimpleEnum.Third };
            IEnumerable newCollection = new CustomCollectionWithAdd<SimpleEnum>();

            source.AddToCollection(newCollection, typeof(SimpleEnum), typeof(CollectionDeserializationHelpersTest), "PropertyName", newCollection.GetType());

            Assert.Equal(new[] { SimpleEnum.First, SimpleEnum.Second, SimpleEnum.Third }, newCollection as IEnumerable<SimpleEnum>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertUtcDateTime()
        {
            // Arrange
            DateTime dt1 = new DateTime(1978, 11, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt2 = new DateTime(2014, 10, 27, 0, 0, 0, DateTimeKind.Utc);
            IList<DateTimeOffset> source = new List<DateTimeOffset> { new DateTimeOffset(dt1), new DateTimeOffset(dt2) };

            IEnumerable<DateTime> expect = source.Select(e => e.LocalDateTime);
            TimeZoneInfoHelper.TimeZone = null;
            IEnumerable newCollection = new CustomCollectionWithAdd<DateTime>();

            // Act
            source.AddToCollection(newCollection, typeof(DateTime), typeof(CollectionDeserializationHelpersTest),
                "PropertyName", newCollection.GetType());

            // Assert
            Assert.Equal(expect, newCollection as IEnumerable<DateTime>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertUtcDateTime_ToDestinationTimeZone()
        {
            // Arrange
            DateTime dt1 = new DateTime(1978, 11, 15, 10, 20, 30, DateTimeKind.Utc);
            DateTime dt2 = new DateTime(2014, 10, 27, 10, 20, 30, DateTimeKind.Utc);
            IList source = new List<DateTimeOffset> { new DateTimeOffset(dt1), new DateTimeOffset(dt2) };
            IEnumerable newCollection = new CustomCollectionWithAdd<DateTime>();
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00 / -7:00

            // Act
            source.AddToCollection(newCollection, typeof(DateTime), typeof(CollectionDeserializationHelpersTest),
                "PropertyName", newCollection.GetType());

            // Assert
            Assert.Equal(new[] { dt1.AddHours(-8), dt2.AddHours(-7) }, newCollection as IEnumerable<DateTime>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertLocalDateTime_ToDestinationTimeZone()
        {
            // Arrange
            DateTimeOffset dto1 = DateTimeOffset.Parse("2014-12-16T01:02:03+8:00");
            DateTimeOffset dto2 = DateTimeOffset.Parse("2014-12-16T01:02:03-2:00");
            IList source = new List<DateTimeOffset> { dto1, dto2 };
            IEnumerable<DateTime> expect = new List<DateTime>
            {
                dto1.LocalDateTime,
                dto2.LocalDateTime
            };
            TimeZoneInfoHelper.TimeZone = null;
            IEnumerable newCollection = new CustomCollectionWithAdd<DateTime>();

            // Act
            source.AddToCollection(newCollection, typeof(DateTime), typeof(CollectionDeserializationHelpersTest),
                "PropertyName", newCollection.GetType());

            // Assert
            Assert.Equal(expect, newCollection as IEnumerable<DateTime>);
        }

        [Fact]
        public void CopyItemsToCollection_CanConvertLocalDateTime()
        {
            // Arrange
            DateTimeOffset dto1 = DateTimeOffset.Parse("2014-12-16T01:02:03+8:00");
            DateTimeOffset dto2 = DateTimeOffset.Parse("2014-12-16T01:02:03-2:00");
            IList source = new List<DateTimeOffset> { dto1, dto2 };
            IEnumerable newCollection = new CustomCollectionWithAdd<DateTime>();
            IEdmModel model = new EdmModel();
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00 / -7:00

            // Act
            source.AddToCollection(newCollection, typeof(DateTime), typeof(CollectionDeserializationHelpersTest),
                "PropertyName", newCollection.GetType());

            // Assert
            Assert.Equal(new[] { new DateTime(2014, 12, 15, 9, 2, 3), new DateTime(2014, 12, 15, 19, 2, 3) },
                newCollection as IEnumerable<DateTime>);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<int>), typeof(int))]
        [InlineData(typeof(ICollection<int>), typeof(int))]
        [InlineData(typeof(IList<int>), typeof(int))]
        [InlineData(typeof(Collection<int>), typeof(int))]
        [InlineData(typeof(List<int>), typeof(int))]
        [InlineData(typeof(LinkedList<int>), typeof(int))]
        public void TryCreateInstance_Creates_AppropriateCollectionObject(Type collectionType, Type elementType)
        {
            IEnumerable result;
            bool created = CollectionDeserializationHelpers.TryCreateInstance(collectionType, null, elementType, out result);

            Assert.True(created);
            Assert.IsAssignableFrom(collectionType, result);
        }

        [Fact]
        public void TryCreateInstance_EdmComplexObjectCollection_SetsEdmType()
        {
            EdmComplexType complexType = new EdmComplexType("NS", "ComplexType");
            IEdmCollectionTypeReference complexCollectionType = 
                new EdmCollectionType(complexType.ToEdmTypeReference(true))
                .ToEdmTypeReference(true).AsCollection();
            
            IEnumerable result;
            CollectionDeserializationHelpers.TryCreateInstance(typeof(EdmComplexObjectCollection), complexCollectionType, typeof(EdmComplexObject), out result);

            var edmObject = Assert.IsType<EdmComplexObjectCollection>(result);
            Assert.Equal(edmObject.GetEdmType(), complexCollectionType, new EdmTypeReferenceEqualityComparer());
        }

        [Fact]
        public void TryCreateInstance_EdmEntityObjectCollection_SetsEdmType()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "EntityType");
            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionType(entityType.ToEdmTypeReference(true))
                .ToEdmTypeReference(true).AsCollection();

            IEnumerable result;
            CollectionDeserializationHelpers.TryCreateInstance(typeof(EdmEntityObjectCollection), entityCollectionType, typeof(EdmComplexObject), out result);

            var edmObject = Assert.IsType<EdmEntityObjectCollection>(result);
            Assert.Equal(edmObject.GetEdmType(), entityCollectionType, new EdmTypeReferenceEqualityComparer());
        }

        private class CustomCollectionWithAdd<T> : IEnumerable<T>
        {
            List<T> _list = new List<T>();

            public void Add(T item)
            {
                _list.Add(item);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
    }
}
