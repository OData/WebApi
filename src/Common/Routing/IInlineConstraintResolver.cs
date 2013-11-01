// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
using ConstraintType = System.Web.Http.Routing.IHttpRouteConstraint;
#else
using ConstraintType = System.Web.Routing.IRouteConstraint;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>
    /// Defines an abstraction for resolving inline constraints as instances of <see cref="ConstraintType"/>.
    /// </summary>
    public interface IInlineConstraintResolver
    {
        /// <summary>
        /// Resolves the inline constraint.
        /// </summary>
        /// <param name="inlineConstraint">The inline constraint to resolve.</param>
        /// <returns>The <see cref="ConstraintType"/> the inline constraint was resolved to.</returns>
        ConstraintType ResolveConstraint(string inlineConstraint);
    }
}