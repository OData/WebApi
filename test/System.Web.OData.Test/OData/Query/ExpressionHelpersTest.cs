// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Linq.Expressions;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    public class ExpressionHelpersTest
    {
        [Fact]
        public void ToNullable_Returns_SameExpressionIfTypeIsAlreadyNullable()
        {
            Expression expression = Expression.Constant("nullable string");

            Assert.Same(expression, ExpressionHelpers.ToNullable(expression));
        }

        [Fact]
        public void ToNullable_Returns_ConvertExpressionIfTypeIsNotNullable()
        {
            Expression expression = Expression.Constant(42);

            Expression result = ExpressionHelpers.ToNullable(expression);

            Assert.Equal(result.NodeType, ExpressionType.Convert);
            Assert.Equal(result.Type, typeof(int?));
        }

        public static TheoryDataSet<Type, object> DefaultValuesTestData
        {
            get
            {
                return new TheoryDataSet<Type, object>
                {
                    { typeof(int), default(int) },
                    { typeof(TestStruct), default(TestStruct) },
                    { typeof(string), default(string) },
                    { typeof(IEnumerable), default(IEnumerable) },
                    { typeof(TestEntity), default(TestEntity) }
                };
            }
        }

        [Theory]
        [PropertyData("DefaultValuesTestData")]
        public void Default_Returns_ConstantExpressionWithExpectedValue(Type type, object expectedValue)
        {
            Expression defaultExpression = ExpressionHelpers.Default(type);

            Assert.Equal(ExpressionType.Constant, defaultExpression.NodeType);
            Assert.Equal(expectedValue, (defaultExpression as ConstantExpression).Value);
        }

        private struct TestStruct
        {
            public int I { get; set; }

            public string S { get; set; }
        }

        private class TestEntity
        {
        }
    }
}
