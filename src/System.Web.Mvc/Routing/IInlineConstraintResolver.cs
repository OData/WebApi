// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Defines an abstraction for resolving inline constraints as instances of <see cref="IRouteConstraint"/>.
    /// </summary>
    public interface IInlineConstraintResolver
    {
        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="IRouteConstraint"/> the inline constraint was resolved to.</returns>
        IRouteConstraint ResolveConstraint(string inlineConstraint);
    }
}