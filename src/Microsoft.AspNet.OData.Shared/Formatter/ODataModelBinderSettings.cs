// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// This class describes the settings to use during model binding.
    /// </summary>
    public class ODataModelBinderSettings : IODataModelBindingSettings
    {
        private bool _enableCaseInsensitiveModelBinding;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ODataModelBinderSettings"/> class
        /// and initializes the default settings.
        /// </summary>
        public ODataModelBinderSettings()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether request body binding should be case insensitive.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        public bool EnableCaseInsensitiveModelBinding
        {
            get
            {
                return _enableCaseInsensitiveModelBinding;
            }
            set
            {
                _enableCaseInsensitiveModelBinding = value;
            }
        }
    }
}
