//-----------------------------------------------------------------------------
// <copyright file="ParameterConfigurationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class ParameterConfigurationTest
    {
        [Fact]
        public void BindingParameterConfigurationThrowsWhenParameterTypeIsNotEntity()
        { 
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<Address>();

            // Act & Assert
            ArgumentException exception = ExceptionAssert.Throws<ArgumentException>(() =>
            {
                BindingParameterConfiguration configuration = new BindingParameterConfiguration("name", builder.GetTypeConfigurationOrNull(typeof(Address)));
            });
            Assert.Contains(string.Format("'{0}'", typeof(Address).FullName), exception.Message);
            Assert.Equal("parameterType", exception.ParamName);
        }

        [Theory]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(double?), true)]
        [InlineData(typeof(Color), false)]
        [InlineData(typeof(Color?), true)]
        [InlineData(typeof(Address), true)]
        [InlineData(typeof(Customer), true)]
        public void NonbindingParameterConfigurationSupportsParameterTypeAs(Type type, bool isNullable)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            builder.ComplexType<Address>();
            builder.EnumType<Color>();

            // Act
            Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(type);
            IEdmTypeConfiguration edmTypeConfiguration = builder.GetTypeConfigurationOrNull(type);
            if (underlyingType.IsEnum)
            {
                edmTypeConfiguration = builder.GetTypeConfigurationOrNull(underlyingType);
                if (edmTypeConfiguration != null && isNullable)
                {
                    edmTypeConfiguration = ((EnumTypeConfiguration)edmTypeConfiguration).GetNullableEnumTypeConfiguration();
                }
            }
            NonbindingParameterConfiguration parameter = new NonbindingParameterConfiguration("name",
                edmTypeConfiguration);

            // Assert
            Assert.Equal(isNullable, parameter.Nullable);
        }

        [Theory]
        [InlineData(typeof(IEnumerable<int>), false)]
        [InlineData(typeof(IEnumerable<int?>), true)]
        [InlineData(typeof(ICollection<Color>), false)]
        [InlineData(typeof(IEnumerable<Color?>), true)]
        [InlineData(typeof(IList<Address>), true)]
        [InlineData(typeof(IList<Customer>), true)]
        public void NonbindingParameterConfigurationSupportsParameterCollectionTypeAs(Type type, bool isNullable)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();
            builder.ComplexType<Address>();
            builder.EnumType<Color>();

            Type elementType;
            Assert.True(TypeHelper.IsCollection(type, out elementType));

            // Act
            Type underlyingType = TypeHelper.GetUnderlyingTypeOrSelf(elementType);
            IEdmTypeConfiguration elementTypeConfiguration = builder.GetTypeConfigurationOrNull(underlyingType);
            CollectionTypeConfiguration collectionType = new CollectionTypeConfiguration(elementTypeConfiguration,
                typeof(IEnumerable<>).MakeGenericType(elementType));

            NonbindingParameterConfiguration parameter = new NonbindingParameterConfiguration("name", collectionType);

            // Assert
            Assert.Equal(isNullable, parameter.Nullable);
        }
    }
}
