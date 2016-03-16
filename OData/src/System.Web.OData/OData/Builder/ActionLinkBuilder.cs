// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Builder
{
    /// <summary>
    /// ActionLinkBuilder can be used to annotate an Action. 
    /// This is how formatters create links to invoke bound actions.
    /// </summary>
    public class ActionLinkBuilder : ProcedureLinkBuilder
    {
        /// <summary>
        /// Create a new ActionLinkBuilder based on an actionLinkFactory.
        /// <remarks>
        /// If the action is not available the actionLinkFactory delegate should return NULL.
        /// </remarks>
        /// </summary>
        /// <param name="linkFactory">The actionLinkFactory this ActionLinkBuilder should use when building links.</param>
        /// <param name="followsConventions">
        /// A value indicating whether the action link factory generates links that follow OData conventions.
        /// </param>
        public ActionLinkBuilder(Func<EntityInstanceContext, Uri> linkFactory, bool followsConventions)
            : base(linkFactory, followsConventions)
        {
        }

        /// <summary>
        /// Create a new ActionLinkBuilder based on an actionLinkFactory.
        /// <remarks>
        /// If the action is not available the actionLinkFactory delegate should return NULL.
        /// </remarks>
        /// </summary>
        /// <param name="linkFactory">The actionLinkFactory this ActionLinkBuilder should use when building links.</param>
        /// <param name="followsConventions">
        /// A value indicating whether the action link factory generates links that follow OData conventions.
        /// </param>
        public ActionLinkBuilder(Func<FeedContext, Uri> linkFactory, bool followsConventions)
            : base(linkFactory, followsConventions)
        {
        }

        /// <summary>
        /// Builds the action link for the given entity.
        /// </summary>
        /// <param name="context">An instance context wrapping the entity instance.</param>
        /// <returns>The generated action link.</returns>
        public virtual Uri BuildActionLink(EntityInstanceContext context)
        {
            return BuildLink(context);
        }

        /// <summary>
        /// Builds the action link for the given feed.
        /// </summary>
        /// <param name="context">An instance context wrapping the feed instance.</param>
        /// <returns>The generated action link.</returns>
        public virtual Uri BuildActionLink(FeedContext context)
        {
            return BuildLink(context);
        }

        /// <summary>
        /// Creates an action link factory that builds an action link, but only when appropriate based on the expensiveAvailabilityCheck, and whether expensive checks should be made,
        /// which is deduced by looking at the EntityInstanceContext.SkipExpensiveActionAvailabilityChecks property.
        /// </summary>
        /// <param name="baseFactory">The action link factory that actually builds links if all checks pass.</param>
        /// <param name="expensiveAvailabilityCheck">The availability check function that is expensive but when called returns whether the action is available.</param>
        /// <returns>The new action link factory.</returns>
        public static Func<EntityInstanceContext, Uri> CreateActionLinkFactory(Func<EntityInstanceContext, Uri> baseFactory, Func<EntityInstanceContext, bool> expensiveAvailabilityCheck)
        {
            return (EntityInstanceContext ctx) =>
            {
                if (ctx.SkipExpensiveAvailabilityChecks)
                {
                    // OData says that if it is too expensive to check availability you should advertise actions
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
