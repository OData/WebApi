// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Settings to use during model binding.
    /// </summary>
    public interface IODataModelBindingSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether request body binding should be case insensitive.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        bool EnableCaseInsensitiveModelBinding { get; set; }
    }
}
