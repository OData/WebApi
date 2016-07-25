// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This class describes the default settings to use during query composition.
    /// </summary>
    public class DefaultQuerySettings
    {
        private bool _enableFilter;
        private bool _enableOrderBy;
        private bool _enableExpand;
        private bool _enableCount;
        private bool _enableSelect;
        private int? _maxTop = 0;

        /// <summary>
        /// Gets or sets a value indicating whether navigation property can be expanded.
        /// </summary>
        public bool EnableExpand 
        {
            get
            {
                return _enableExpand;
            }
            set
            {
                _enableExpand = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether property can be selected.
        /// </summary>
        public bool EnableSelect
        {
            get
            {
                return _enableSelect;
            }
            set
            {
                _enableSelect = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether entity set and property can apply $count.
        /// </summary>
        public bool EnableCount
        {
            get
            {
                return _enableCount;
            }
            set
            {
                _enableCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether property can apply $orderby.
        /// </summary>
        public bool EnableOrderBy
        {
            get
            {
                return _enableOrderBy;
            }
            set
            {
                _enableOrderBy = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether property can apply $filter.
        /// </summary>
        public bool EnableFilter
        {
            get
            {
                return _enableFilter;
            }
            set
            {
                _enableFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        /// <value>
        /// The max value of $top that a client can request, or <c>null</c> if there is no limit.
        /// </value>
        public int? MaxTop
        {
            get
            {
                return _maxTop;
            }
            set
            {
                if (value.HasValue && value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 0);
                }

                _maxTop = value;
            }
        }
    }
}
