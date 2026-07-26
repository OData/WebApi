//-----------------------------------------------------------------------------
// <copyright file="ODataOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.Logging;
using Microsoft.OData;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public class ODataOptions
    {
        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not in DefaultODataPathHandler.
        /// </summary>
        /// <remarks>Default value is unspecified (null).</remarks>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating if value should be emitted for dynamic properties which are null.
        /// </summary>
        public bool NullDynamicPropertyIsEnabled { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating if batch requests should continue on error.
        /// </summary>
        public bool EnableContinueOnErrorHeader { get; set; }

        /// <summary>
        /// Gets or Sets the set of flags that have options for backward compatibility
        /// </summary>
        public CompatibilityOptions CompatibilityOptions { get; set; }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of an OData request or response message body.
        /// Default is 100 MB (104,857,600 bytes). Must be greater than or equal to 1.
        /// </summary>
        public long MaxReceivedMessageSize
        {
            get => _messageSizeOptions.MaxReceivedMessageSize;
            set => _messageSizeOptions.MaxReceivedMessageSize = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether diagnostic details are logged when a query fails validation,
        /// for actions annotated with <see cref="EnableQueryAttribute"/>. Provides the default for every such
        /// action; an individual <see cref="EnableQueryAttribute.EnableQueryValidationErrorLogging"/> overrides it.
        /// The default value is <c>false</c>.
        /// </summary>
        public bool EnableQueryValidationErrorLogging { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogLevel"/> at which query validation diagnostics are written when
        /// <see cref="EnableQueryValidationErrorLogging"/> is enabled. The default value is <see cref="LogLevel.Warning"/>.
        /// </summary>
        public LogLevel QueryValidationErrorLogLevel { get; set; } = LogLevel.Warning;

        private readonly ODataMessageSizeOptions _messageSizeOptions = new ODataMessageSizeOptions();
    }
}
