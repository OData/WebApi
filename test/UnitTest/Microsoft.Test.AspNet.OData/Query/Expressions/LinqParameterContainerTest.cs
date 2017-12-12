// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.AspNet.OData.Query.Expressions;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Query.Expressions
{
    public class LinqParameterContainerTest
    {
        [Theory]
        [InlineData("42")]
        [InlineData(42)]
        [InlineData(42.0)]
        [InlineData(true)]
        public void Parameterize_ProducesPropertyAccessOnConstant(object value)
        {
            Expression expr = LinqParameterContainer.Parameterize(value.GetType(), value);

            LinqParameterContainer parameterizedValue = ((expr as MemberExpression).Expression as ConstantExpression).Value as LinqParameterContainer;
            Assert.Equal(value, parameterizedValue.Property);
        }
    }
}
