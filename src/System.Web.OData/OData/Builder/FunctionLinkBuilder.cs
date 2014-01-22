// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// FunctionLinkBuilder can be used to annotate an Function.
    /// This is how formatters create links to invoke bound functions.
    /// </summary>
    public class FunctionLinkBuilder
    {
        private Func<EntityInstanceContext, Uri> _functionLinkFactory;

        /// <summary>
        /// Create a new FunctionLinkBuilder based on an functionLinkFactory.
        /// <remarks>
        /// If the function is not available the functionLinkFactory delegate should return NULL.
        /// </remarks>
        /// </summary>
        /// <param name="functionLinkFactory">The functionLinkFactory this FunctionLinkBuilder should use when building links.</param>
        /// <param name="followsConventions">
        /// A value indicating whether the function link factory generates links that follow OData conventions.
        /// </param>
        public FunctionLinkBuilder(Func<EntityInstanceContext, Uri> functionLinkFactory, bool followsConventions)
        {
            if (functionLinkFactory == null)
            {
                throw Error.ArgumentNull("functionLinkFactory");
            }

            _functionLinkFactory = functionLinkFactory;
            FollowsConventions = followsConventions;
        }

        /// <summary>
        /// Gets a boolean indicating whether the link factory follows OData conventions or not.
        /// </summary>
        public bool FollowsConventions { get; private set; }

        /// <summary>
        /// Builds the function link for the given entity.
        /// </summary>
        /// <param name="context">An instance context wrapping the entity instance.</param>
        /// <returns>The generated function link.</returns>
        public virtual Uri BuildFunctionLink(EntityInstanceContext context)
        {
            return _functionLinkFactory(context);
        }

        /// <summary>
        /// Creates an function link factory that builds an function link, but only when appropriate based on the expensiveAvailabilityCheck, and whether expensive checks should be made,
        /// which is deduced by looking at the EntityInstanceContext.SkipExpensiveFunctionAvailabilityChecks property.
        /// </summary>
        /// <param name="baseFactory">The function link factory that actually builds links if all checks pass.</param>
        /// <param name="expensiveAvailabilityCheck">The availability check function that is expensive but when called returns whether the function is available.</param>
        /// <returns>The new function link factory.</returns>
        public static Func<EntityInstanceContext, Uri> CreateFunctionLinkFactory(Func<EntityInstanceContext, Uri> baseFactory, Func<EntityInstanceContext, bool> expensiveAvailabilityCheck)
        {
            return (EntityInstanceContext ctx) =>
            {
                if (ctx.SkipExpensiveAvailabilityChecks)
                {
                    // OData says that if it is too expensive to check availability you should advertize functions
                    return baseFactory(ctx);
                }
                else if (expensiveAvailabilityCheck(ctx))
                {
                    return baseFactory(ctx);
                }
                else
                {
                    return null;
                }
            };
        }
    }
}
