// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This class describes the settings to use during query composition.
    /// </summary>
    public class ODataQuerySettings
    {
        private HandleNullPropagationOption _handleNullPropagationOption = HandleNullPropagationOption.Default;
        private int? _pageSize;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ODataQuerySettings"/> class
        /// and initializes the default settings.
        /// </summary>
        public ODataQuerySettings()
        {
            EnsureStableOrdering = true;
            EnableConstantParameterization = true;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="ODataQuerySettings"/> class based on an existing one. 
        /// </summary>
        /// <param name="settings">The setting to copy from.</param>
        public ODataQuerySettings(ODataQuerySettings settings)
        {
            EnsureStableOrdering = settings.EnsureStableOrdering;
            EnableConstantParameterization = settings.EnableConstantParameterization;
            HandleNullPropagation = settings.HandleNullPropagation;
            PageSize = settings.PageSize;
        }

        /// <summary>
        /// Gets or sets a value indicating whether query composition should
        /// alter the original query when necessary to ensure a stable sort order.
        /// </summary>
        /// <value>A <c>true</c> value indicates the original query should
        /// be modified when necessary to guarantee a stable sort order.
        /// A <c>false</c> value indicates the sort order can be considered
        /// stable without modifying the query.  Query providers that ensure
        /// a stable sort order should set this value to <c>false</c>.
        /// The default value is <c>true</c>.</value>
        public bool EnsureStableOrdering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition. 
        /// </summary>
        /// <value>
        /// The default is <see cref="HandleNullPropagationOption.Default"/>.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation
        {
            get
            {
                return _handleNullPropagationOption;
            }
            set
            {
                HandleNullPropagationOptionHelper.Validate(value, "value");
                _handleNullPropagationOption = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants 
        /// would result in better performance with Entity framework.
        /// </summary>
        /// <value>The default value is <c>true</c>.</value>
        public bool EnableConstantParameterization { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of query results to return.
        /// </summary>
        /// <value>
        /// The maximum number of query results to return, or <c>null</c> if there is no limit.
        /// </value>
        public int? PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value.HasValue && value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
                }

                _pageSize = value;
            }
        }
    }
}
