// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Defines an abstraction for resolving inline constraints as instances of <see cref="IHttpRouteConstraint"/>.
    /// </summary>
    public interface IInlineConstraintResolver
    {
        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="IHttpRouteConstraint"/> the inline constraint was resolved to.</returns>
        IHttpRouteConstraint ResolveConstraint(string inlineConstraint);
    }
}