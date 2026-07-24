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
        /// Gets or sets a value indicating whether diagnostic details are recorded when an incoming query
        /// fails validation for actions annotated with <see cref="EnableQueryAttribute"/>. When enabled, the
        /// endpoint's route template, the queried type, the requested <c>$select</c> and <c>$expand</c>, and the failure
        /// reason are written. This value provides the default for every such action, allowing the behavior to be
        /// configured once instead of on each attribute; an individual <see cref="EnableQueryAttribute"/> overrides it
        /// by setting <see cref="EnableQueryAttribute.EnableQueryValidationErrorLogging"/> explicitly. The default
        /// value is <c>false</c>.
        /// </summary>
        public bool EnableQueryValidationErrorLogging { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogLevel"/> at which query validation diagnostics are written for actions
        /// annotated with <see cref="EnableQueryAttribute"/> when <see cref="EnableQueryValidationErrorLogging"/> is
        /// enabled. This value applies to every such action, so the level can be configured once. The default value is
        /// <see cref="LogLevel.Warning"/>.
        /// </summary>
        public LogLevel QueryValidationErrorLogLevel { get; set; } = LogLevel.Warning;

        private readonly ODataMessageSizeOptions _messageSizeOptions = new ODataMessageSizeOptions();
    }
}
