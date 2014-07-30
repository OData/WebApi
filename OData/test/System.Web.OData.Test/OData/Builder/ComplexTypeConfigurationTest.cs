// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Builder.Test
{
    public class ComplexTypeConfigurationTest
    {
        [Theory]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTime?))]
        public void Ctor_ThrowsIfPropertyIsDateTime(Type type)
        {
            // Act & Assert
            Assert.ThrowsArgument(() =>
                new ComplexTypeConfiguration(Mock.Of<ODataModelBuilder>(), type),
                "clrType",
                string.Format("The type '{0}' is not a supported type.", type.FullName));
        }
    }
}
