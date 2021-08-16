//-----------------------------------------------------------------------------
// <copyright file="PageAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property or a class to specify that
    /// the maximum value of $top and query result return number of that property or type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class PageAttribute : Attribute
    {
        private int _maxTop = -1;

        /// <summary>
        /// Sets the max value of $top that a client can request.
        /// </summary>
        public int MaxTop 
        {
            get
            {
                return _maxTop;
            }
            set
            {
                _maxTop = value;
            } 
        }

        /// <summary>
        /// Sets the maximum number of query results to return.
        /// </summary>
        public int PageSize { get; set; }
    }
}
