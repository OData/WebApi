//-----------------------------------------------------------------------------
// <copyright file="ClrTypeCacheTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ClrTypeCacheTest
    {
        private static IEdmModel _model = GetEdmModel();

        #region GetEdmType
        [Fact]
        public void GetEdmType_Returns_CachedInstance()
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            IEdmModel model = EdmCoreModel.Instance;

            // Act
            IEdmTypeReference edmType1 = cache.GetEdmType(typeof(int), model);
            IEdmTypeReference edmType2 = cache.GetEdmType(typeof(int), model);

            // Assert
            Assert.NotNull(edmType1);
            Assert.Same(edmType1, edmType2);
        }

        [Fact]
        public void GetEdmType_Returns_CachedInstance_ForClrType()
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            Type clrType = typeof(CacheAddress);

            // Act
            IEdmTypeReference edmType1 = cache.GetEdmType(clrType, _model);
            IEdmTypeReference edmType2 = cache.GetEdmType(clrType, _model);

            // Assert
            Assert.NotNull(edmType1);
            Assert.Same(edmType1, edmType2);
            Assert.Equal("NS.Address", edmType1.FullName());
            Assert.True(edmType1.IsNullable);
        }

        [Fact]
        public void GetEdmType_Cached_OnlyOneInstance()
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            Type clrType = typeof(CacheAddress);
            Action cacheCallAndVerify = () =>
            {
                IEdmTypeReference edmType = cache.GetEdmType(clrType, _model);
                Assert.NotNull(edmType);
                Assert.Equal("NS.Address", edmType.FullName());

                Assert.Single(cache.ClrToEdmTypeCache);
            };

            // Act & Assert
            cacheCallAndVerify();

            // 5 is a magic number, it doesn't matter, just want to call it mulitple times.
            for (int i = 0; i < 5; i++)
            {
                cacheCallAndVerify();
            }

            cacheCallAndVerify();
        }

        #endregion

        #region GetClrType

        [Fact]
        public void GetClrType_Returns_CorrectType()
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            Type clrType = typeof(CacheAddress);
            IEdmComplexType address = _model.SchemaElements.OfType<IEdmComplexType>().FirstOrDefault(c => c.Name == "Address");

            // Act & Assert
            IEdmComplexTypeReference addressType = new EdmComplexTypeReference(address, true);
            Type actual = cache.GetClrType(addressType, _model);
            Assert.Same(clrType, actual);

            addressType = new EdmComplexTypeReference(address, false);
            actual = cache.GetClrType(addressType, _model);
            Assert.Same(clrType, actual);
        }

        [Fact]
        public void GetClrType_Cached_OnlyOneInstance()
        {
            // Arrange
            ClrTypeCache cache = new ClrTypeCache();
            Type clrType = typeof(CacheAddress);
            IEdmComplexType address = _model.SchemaElements.OfType<IEdmComplexType>().FirstOrDefault(c => c.Name == "Address");

            Action cacheCallAndVerify = () =>
            {
                IEdmComplexTypeReference addressTypeTrue = new EdmComplexTypeReference(address, true);
                Type actual = cache.GetClrType(addressTypeTrue, _model);
                Assert.Same(clrType, actual);

                IEdmComplexTypeReference addressTypeFalse = new EdmComplexTypeReference(address, false);
                actual = cache.GetClrType(addressTypeFalse, _model);
                Assert.Same(clrType, actual);

                Assert.Equal(2, cache.EdmToClrTypeCache.Count);
            };

            // Act & Assert
            cacheCallAndVerify();

            // 5 is a magic number, it doesn't matter, just want to call it mulitple times.
            for (int i = 0; i < 5; i++)
            {
                cacheCallAndVerify();
            }

            cacheCallAndVerify();
        }
        #endregion

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Address is an open complex type
            var addressType = new EdmComplexType("NS", "Address", null, false, true);
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(addressType);

            IEdmComplexType edmType = model.FindDeclaredType("NS.Address") as IEdmComplexType;
            Type cacheAddressType = typeof(CacheAddress);
            model.SetAnnotationValue(edmType, new ClrTypeAnnotation(cacheAddressType));
            return model;
        }

        private class CacheAddress
        {
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}
