// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Builder
{
    internal class PropertySelectorVisitor : ExpressionVisitor
    {
        private List<PropertyInfo> _properties = new List<PropertyInfo>();

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Class is internal, virtual call okay")]
        internal PropertySelectorVisitor(Expression exp)
        {
            Visit(exp);
        }

        public PropertyInfo Property
        {
            get
            {
                return _properties.SingleOrDefault();
            }
        }

        public ICollection<PropertyInfo> Properties
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
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeProperties, node.Member.ReflectedType.FullName, node.Member.Name);
            }

            if (node.Expression.NodeType != ExpressionType.Parameter)
            {
                throw Error.InvalidOperation(SRResources.MemberExpressionsMustBeBoundToLambdaParameter);
            }

            _properties.Add(pinfo);
            return node;
        }

        public static PropertyInfo GetSelectedProperty(Expression exp)
        {
            return new PropertySelectorVisitor(exp).Property;
        }

        public static ICollection<PropertyInfo> GetSelectedProperties(Expression exp)
        {
            return new PropertySelectorVisitor(exp).Properties;
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
