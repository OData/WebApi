// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.OData.Query.Expressions
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
