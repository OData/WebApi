//-----------------------------------------------------------------------------
// <copyright file="PropertyPairSelectorVisitor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    internal class PropertyPairSelectorVisitor : ExpressionVisitor
    {
        private readonly IDictionary<PropertyInfo, PropertyInfo> _properties =
            new Dictionary<PropertyInfo, PropertyInfo>();

        public IDictionary<PropertyInfo, PropertyInfo> Properties
        {
            get { return _properties; }
        }

        public static IDictionary<PropertyInfo, PropertyInfo> GetSelectedProperty(Expression exp)
        {
            PropertyPairSelectorVisitor visitor = new PropertyPairSelectorVisitor();
            visitor.Visit(exp);
            return visitor.Properties;
        }
        
        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return base.Visit(exp);

                case ExpressionType.And: // &
                case ExpressionType.AndAlso: // &&
                    BinaryExpression node = (BinaryExpression)exp;
                    Visit(node.Left);
                    return Visit(node.Right);

                case ExpressionType.Equal: // ==
                    return VisitEqual(exp);

                default:
                    throw Error.NotSupported(SRResources.UnsupportedExpressionNodeTypeWithName, exp.NodeType);
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> lambda)
        {
            if (lambda == null)
            {
                throw Error.ArgumentNull("lambda");
            }

            if (lambda.Parameters.Count != 2)
            {
                throw Error.InvalidOperation(SRResources.LambdaExpressionMustHaveExactlyTwoParameters);
            }

            Expression body = Visit(lambda.Body);

            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }

            return lambda;
        }

        private Expression VisitEqual(Expression exp)
        {
            Contract.Assert(exp != null && exp.NodeType == ExpressionType.Equal);

            BinaryExpression node = (BinaryExpression)exp;

            PropertyInfo left = VisitMemberProperty(node.Left);
            PropertyInfo right = VisitMemberProperty(node.Right);

            if (left != null && right != null)
            {
                Type leftType = Nullable.GetUnderlyingType(left.PropertyType) ?? left.PropertyType;
                Type rightType = Nullable.GetUnderlyingType(right.PropertyType) ?? right.PropertyType;
                if (leftType != rightType)
                {
                    throw Error.InvalidOperation(SRResources.EqualExpressionsMustHaveSameTypes,
                        TypeHelper.GetReflectedType(left).FullName, left.Name, left.PropertyType.FullName,
                        TypeHelper.GetReflectedType(right).FullName, right.Name, right.PropertyType.FullName);
                }

                _properties.Add(left, right);
            }

            return exp;
        }

        private PropertyInfo VisitMemberProperty(Expression node)
        {
            Contract.Assert(node != null);

            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return GetPropertyInfo((MemberExpression)node);

                case ExpressionType.Convert:
                    return VisitMemberProperty(((UnaryExpression)node).Operand);
            }

            return null;
        }

        private static PropertyInfo GetPropertyInfo(MemberExpression memberNode)
        {
            Contract.Assert(memberNode != null);

            PropertyInfo propertyInfo = memberNode.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties,
                    TypeHelper.GetReflectedType(memberNode.Member).FullName, memberNode.Member.Name);
            }

            if (memberNode.Expression.NodeType != ExpressionType.Parameter)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            }

            return propertyInfo;
        }
    }
}
