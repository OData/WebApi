// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData
{
    /// <summary>
    /// Represents an annotation to add the queryable restrictions on an EDM property, including nonfilterable, unsortable, not navigable, not expandable.
    /// </summary>
    public class QueryableRestrictionsAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="QueryableRestrictionsAnnotation"/> class.
        /// </summary>
        /// <param name="restrictions">The queryable restrictions for the EDM property.</param>
        public QueryableRestrictionsAnnotation(QueryableRestrictions restrictions)
        {
            if (restrictions == null)
            {
                throw Error.ArgumentNull("restrictions");
            }

            Restrictions = restrictions;
        }

        /// <summary>
        /// Gets the restrictions for the EDM property.
        /// </summary>
        public QueryableRestrictions Restrictions { get; private set; }
    }
}
