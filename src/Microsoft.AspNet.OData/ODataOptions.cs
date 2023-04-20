//-----------------------------------------------------------------------------
// <copyright file="ODataOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public class ODataOptions
    {
        /// <summary>
        /// The default instance of <see cref="ODataOptions"/>.
        /// </summary>
        public static ODataOptions Default { get; } = new ODataOptions();

        /// <summary>
        /// Gets or sets a value indicating if <see cref="ODataQueryOptions"/> should be reused between
        /// <see cref="ODataQueryParameterBindingAttribute"/> and <see cref="EnableQueryAttribute"/>.
        /// </summary>
        public bool ParseODataQueryOptionsOnce { get; set; }
    }
}
