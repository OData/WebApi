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
        public void Ctor_DoesnotThrows_IfPropertyIsDateTime(Type type)
        {
            Assert.DoesNotThrow(() => new ComplexTypeConfiguration(Mock.Of<ODataModelBuilder>(), type));
        }
    }
}
