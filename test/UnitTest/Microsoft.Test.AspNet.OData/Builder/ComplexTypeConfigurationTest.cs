// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData.Buildert
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
