//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeConfigurationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Buildert
{
    public class ComplexTypeConfigurationTest
    {
        [Theory]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTime?))]
        public void Ctor_DoesnotThrows_IfPropertyIsDateTime(Type type)
        {
            ExceptionAssert.DoesNotThrow(() => new ComplexTypeConfiguration(Mock.Of<ODataModelBuilder>(), type));
        }
    }
}
