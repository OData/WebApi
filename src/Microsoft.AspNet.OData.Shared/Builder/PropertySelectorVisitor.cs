// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    internal class PropertySelectorVisitor : ExpressionVisitor
    {
        private List<MemberInfo> _properties = new List<MemberInfo>();
        private readonly bool includeExtensionProperty;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Class is internal, virtual call okay")]
        internal PropertySelectorVisitor(Expression exp, bool includeExtensionProperty)
        {
            this.includeExtensionProperty = includeExtensionProperty;
            Visit(exp);
        }

        public MemberInfo Property
        {
            get
            {
                return _properties.SingleOrDefault();
            }
        }

        public ICollection<MemberInfo> Properties
        {
            get
            {
                return _properties;
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            PropertyInfo pinfo = node.Member as PropertyInfo;

            if (pinfo == null)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties, TypeHelper.GetReflectedType(node.Member).FullName, node.Member.Name);
            }

            if (node.Expression.NodeType != ExpressionType.Parameter)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            }

            _properties.Add(pinfo);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull("node");
            }

            MethodInfo minfo = node.Method;

            if (!IsExtensionProperty(minfo))
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties, TypeHelper.GetReflectedType(node.Method).FullName, node.Method.Name);
            }

            if (node.Arguments.First().NodeType != ExpressionType.Parameter)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            }

            _properties.Add(minfo);
            return node;
        }

        private static bool IsExtensionProperty(MethodInfo methodInfo)
        {
            return methodInfo!=null
                && methodInfo.IsStatic
                && methodInfo.IsDefined(typeof(ExtensionAttribute), false)
                && methodInfo.GetParameters().Length == 1;
        }

        public static PropertyInfo GetSelectedProperty(Expression exp)
        {
            return GetSelectedProperty(exp, false) as PropertyInfo;
        }

        public static MemberInfo GetSelectedProperty(Expression exp, bool includeExtensionProperty)
        {
            return new PropertySelectorVisitor(exp, includeExtensionProperty).Property;
        }

        public static ICollection<MemberInfo> GetSelectedProperties(Expression exp, bool includeExtensionProperty=false)
        {
            return new PropertySelectorVisitor(exp, includeExtensionProperty).Properties;
        }

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return exp;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.New:
                case ExpressionType.MemberAccess:
                case ExpressionType.Lambda:
                    return base.Visit(exp);
                case ExpressionType.Call:
                    if (includeExtensionProperty)
                        return base.Visit(exp);
                    else
                        goto default;
                default:
                    throw Error.NotSupported(SRResources.UnsupportedExpressionNodeType);
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> lambda)
        {
            if (lambda == null)
            {
                throw Error.ArgumentNull("lambda");
            }

            if (lambda.Parameters.Count != 1)
            {
                throw Error.InvalidOperation(SRResources.LambdaExpressionMustHaveExactlyOneParameter);
            }

            Expression body = Visit(lambda.Body);

            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }
    }
}
