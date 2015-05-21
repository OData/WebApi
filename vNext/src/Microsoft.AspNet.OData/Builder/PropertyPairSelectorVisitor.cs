// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;


namespace System.Web.OData.Builder
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

                //default:
                //    throw Error.NotSupported(SRResources.UnsupportedExpressionNodeTypeWithName, exp.NodeType);
            }

            return null;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambda)
        {
            if (lambda == null)
            {
                throw Error.ArgumentNull("lambda");
            }

            //if (lambda.Parameters.Count != 2)
            //{
            //    throw Error.InvalidOperation(SRResources.LambdaExpressionMustHaveExactlyTwoParameters);
            //}

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
                if (left.PropertyType != right.PropertyType)
                {
                    //throw Error.InvalidOperation(SRResources.EqualExpressionsMustHaveSameTypes,
                    //    left.ReflectedType.FullName, left.Name, left.PropertyType.FullName,
                    //    right.ReflectedType.FullName, right.Name, right.PropertyType.FullName);
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
            //if (propertyInfo == null)
            //{
            //    throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties,
            //        memberNode.Member.ReflectedType.FullName, memberNode.Member.Name);
            //}

            //if (memberNode.Expression.NodeType != ExpressionType.Parameter)
            //{
            //    throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            //}

            return propertyInfo;
        }
    }
}
