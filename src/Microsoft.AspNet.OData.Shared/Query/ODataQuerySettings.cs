// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This class describes the settings to use during query composition.
    /// </summary>
    public class ODataQuerySettings
    {
        private HandleNullPropagationOption _handleNullPropagationOption = HandleNullPropagationOption.Default;
        private int? _pageSize;
        private int? _modelBoundPageSize;

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
        /// Indicates that SelectExpandWrapper&gt;&lt;.Instance will always be populated with the object being wrapped. This
        /// behavior is not the default because it causes performance issues with Entity Framework.
        /// </summary>
        public bool EnableDeterministicSelectExpandWrapperInstance { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of query results to return based on the type or property.
        /// </summary>
        /// <value>
        /// The maximum number of query results to return based on the type or property,
        /// or <c>null</c> if there is no limit.
        /// </value>
        internal int? ModelBoundPageSize
        {
            get
            {
                return _modelBoundPageSize;
            }
            set
            {
                if (value.HasValue && value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
                }

                _modelBoundPageSize = value;
            }
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
        /// Gets or sets a value indicating whether queries with expanded navigations should be formulated
        /// to encourage correlated subquery results to be buffered.
        /// Buffering correlated subquery results can reduce the number of queries from N + 1 to 2
        /// by buffering results from the subquery.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        public bool EnableCorrelatedSubqueryBuffering { get; set; }

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

        /// <summary>
        /// Honor $filter inside $expand of non-collection navigation property.
        /// The expanded property is only populated when the filter evaluates to true.
        /// This setting is false by default.
        /// </summary>
        public bool HandleReferenceNavigationPropertyExpandFilter { get; set; }

        internal void CopyFrom(ODataQuerySettings settings)
        {
            EnsureStableOrdering = settings.EnsureStableOrdering;
            EnableConstantParameterization = settings.EnableConstantParameterization;
            HandleNullPropagation = settings.HandleNullPropagation;
            PageSize = settings.PageSize;
            ModelBoundPageSize = settings.ModelBoundPageSize;
            HandleReferenceNavigationPropertyExpandFilter = settings.HandleReferenceNavigationPropertyExpandFilter;
            EnableCorrelatedSubqueryBuffering = settings.EnableCorrelatedSubqueryBuffering;
        }
    }
}
