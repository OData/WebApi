// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http.OData;

namespace System.Web.Http
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="QueryableAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    [Obsolete("This class is obsolete; use the EnableQueryAttribute class from the System.Web.Http.OData or " +
        "System.Web.OData namespace.")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want to be able to subclass this type.")]
    public class QueryableAttribute : EnableQueryAttribute
    {
        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public QueryableAttribute() : base()
        {
        }
    }
}