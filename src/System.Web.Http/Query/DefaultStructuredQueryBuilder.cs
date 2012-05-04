// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Query
{
    /// <summary>
    /// The <see cref="DefaultStructuredQueryBuilder" /> understands $filter, $orderby, $top and $skip
    /// OData query parameters
    /// </summary>
    public class DefaultStructuredQueryBuilder : IStructuredQueryBuilder
    {
        /// <summary>
        /// Build the <see cref="StructuredQuery"/> for the given uri.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to build the <see cref="StructuredQuery"/> from</param>
        /// <returns>The <see cref="StructuredQuery"/></returns>
        public virtual StructuredQuery GetStructuredQuery(Uri uri)
        {
            return ODataQueryDeserializer.GetStructuredQuery(uri);
        }
    }
}
