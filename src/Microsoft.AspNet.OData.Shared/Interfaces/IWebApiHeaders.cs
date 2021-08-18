//-----------------------------------------------------------------------------
// <copyright file="IWebApiHeaders.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Represents the collection of Request Headers as defined in RFC 2616.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiHeaders
    {
        /// <summary>
        /// Return if a specified header and specified values are stored in the collection.
        /// </summary>
        /// <param name="key">The specified header.</param>
        /// <param name="values">The specified header values.</param>
        /// <returns>true is the specified header name and values are stored in the collection; otherwise false.</returns>
        bool TryGetValues(string key, out IEnumerable<string> values);
    }
}
