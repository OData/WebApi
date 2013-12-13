// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class PropertySelectorVisitorTest
    {
        [Fact]
        public void CanGetSingleSelectedProperty()
        {
            Expression<Func<AddressEntity, int>> expr = a => a.ID;
            var properties = PropertySelectorVisitor.GetSelectedProperties(expr).ToArray();
            Assert.Equal(1, properties.Length);
            Assert.Equal("ID", properties[0].Name);
        }

        [Fact]
        public void CanGetMultipleSelectedProperties()
        {
            var expr = Expr((AddressEntity a) => new { a.ID, a.ZipCode });
            var properties = PropertySelectorVisitor.GetSelectedProperties(expr).ToArray();
            Assert.Equal(2, properties.Length);
            Assert.Equal("ID", properties[0].Name);
            Assert.Equal("ZipCode", properties[1].Name);
        }

        [Fact]
        public void FailWhenLambdaExpressionAccessesFields()
        {
            Expression<Func<WorkItem, int>> expr = w => w.Field;
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var properties = PropertySelectorVisitor.GetSelectedProperties(expr);
            });
            Assert.Equal(string.Format("Member '{0}.Field' is not a property.", typeof(WorkItem).FullName), exception.Message);
        }

        [Fact]
        public void FailWhenLambdaExpressionHasMoreThanOneParameter()
        {
            Expression<Func<AddressEntity, AddressEntity, int>> expr = (a1, a2) => a1.ID;
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var properties = PropertySelectorVisitor.GetSelectedProperties(expr);
            });
            Assert.Equal("The LambdaExpression must have exactly one parameter.", exception.Message);
        }

        [Fact]
        public void FailWhenMemberExpressionNotBoundToParameter()
        {
            Expression<Func<AddressEntity, int>> expr = (a) => new AddressEntity().ID;
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var properties = PropertySelectorVisitor.GetSelectedProperties(expr);
            });
            Assert.Equal("MemberExpressions must be bound to the LambdaExpression parameter.", exception.Message);
        }

        [Fact]
        public void FailOnUnsupportedExpressionNodeType()
        {
            Expression<Func<AddressEntity, AddressEntity>> expr = (a) => CreateAddress(a.ID);
            var exception = Assert.Throws<NotSupportedException>(() =>
            {
                var properties = PropertySelectorVisitor.GetSelectedProperties(expr);
            });
            Assert.Equal("Unsupported Expression NodeType.", exception.Message);
        }

        private AddressEntity CreateAddress(int id)
        {
            return new AddressEntity { ID = id };
        }

        /// <summary>
        /// This silly method is just here to allow me to create an Expression for a Func returns an anonymous type
        /// so it can be held in a 'var'. 
        /// </summary>
        private static Expression<Func<TEntity, TProjection>> Expr<TEntity, TProjection>(Expression<Func<TEntity, TProjection>> select)
        {
            return select;
        }
    }
}
