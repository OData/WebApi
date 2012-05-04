// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Query
{
    /// <summary>
    /// A <see cref="IStructuredQueryBuilder"/> is used to extract the query from a Uri.
    /// </summary>
    public interface IStructuredQueryBuilder
    {
        /// <summary>
        /// Build the <see cref="StructuredQuery"/> for the given uri. Return null if there is no query 
        /// in the Uri.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> to build the <see cref="StructuredQuery"/> from</param>
        /// <returns>The <see cref="StructuredQuery"/></returns>
        StructuredQuery GetStructuredQuery(Uri uri);
    }
}
