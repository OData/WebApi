// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Web.Http.Query
{
    // TODO: ability to extend the composer. Bug 322919

    /// <summary>
    /// Used to compose two separate queries into a single query
    /// </summary>
    internal static class QueryComposer
    {
        /// <summary>
        /// Composes the specified query with the source provided.
        /// </summary>
        /// <param name="source">The root or source query</param>
        /// <param name="query">The query to compose</param>
        /// <returns>The composed query</returns>
        public static IQueryable Compose(IQueryable source, IQueryable query)
        {
            return QueryRebaser.Rebase(source, query);
        }

        /// <summary>
        /// Class used to insert a specified query source into another separate
        /// query, effectively "rebasing" the query source.
        /// </summary>
        public class QueryRebaser : ExpressionVisitor
        {
            /// <summary>
            /// Rebase the specified query to the specified source
            /// </summary>
            /// <param name="source">The query source</param>
            /// <param name="query">The query to rebase</param>
            /// <returns>Returns the edited query.</returns>
            public static IQueryable Rebase(IQueryable source, IQueryable query)
            {
                Visitor v = new Visitor(source.Expression);
                Expression expr = v.Visit(query.Expression);
                return source.Provider.CreateQuery(expr);
            }

            private class Visitor : ExpressionVisitor
            {
                private Expression _root;

                public Visitor(Expression root)
                {
                    _root = root;
                }

                protected override Expression VisitMethodCall(MethodCallExpression m)
                {
                    if ((m.Arguments.Count > 0 && m.Arguments[0].NodeType == ExpressionType.Constant) &&
                        (((ConstantExpression)m.Arguments[0]).Value != null) &&
                        (((ConstantExpression)m.Arguments[0]).Value is IQueryable))
                    {
                        // we found the innermost source which we replace with the
                        // specified source
                        List<Expression> exprs = new List<Expression>();
                        exprs.Add(_root);
                        exprs.AddRange(m.Arguments.Skip(1));
                        return Expression.Call(m.Method, exprs.ToArray());
                    }
                    return base.VisitMethodCall(m);
                }
            }
        }
    }
}
