//-----------------------------------------------------------------------------
// <copyright file="ODataMessageSizeOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides message size configuration for OData request and response handling.
    /// </summary>
    public class ODataMessageSizeOptions
    {
        /// <summary>Default maximum received message size: 100 MB (104,857,600 bytes).</summary>
        public const long DefaultMaxReceivedMessageSize = 100 * 1024 * 1024;

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of an OData request or response message body.
        /// Default is 100 MB (104,857,600 bytes). Must be greater than or equal to 1.
        /// </summary>
        public long MaxReceivedMessageSize
        {
            get => _maxReceivedMessageSize;
            set
            {
                if (value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(value), value, 1);
                }

                _maxReceivedMessageSize = value;
            }
        }

        private long _maxReceivedMessageSize = DefaultMaxReceivedMessageSize;
    }
}
