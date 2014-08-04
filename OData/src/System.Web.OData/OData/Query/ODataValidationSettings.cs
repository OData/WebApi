// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Web.Http;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This class describes the validation settings for querying.
    /// </summary>
    public class ODataValidationSettings
    {
        private const int MinMaxSkip = 0;
        private const int MinMaxTop = 0;
        private const int MinMaxExpansionDepth = 0;
        private const int MinMaxNodeCount = 1;
        private const int MinMaxAnyAllExpressionDepth = 1;
        private const int MinMaxOrderByNodeCount = 1;
        internal const int DefaultMaxExpansionDepth = 2;

        private AllowedArithmeticOperators _allowedArithmeticOperators = AllowedArithmeticOperators.All;
        private AllowedFunctions _allowedFunctions = AllowedFunctions.AllFunctions;
        private AllowedLogicalOperators _allowedLogicalOperators = AllowedLogicalOperators.All;
        private AllowedQueryOptions _allowedQueryParameters = AllowedQueryOptions.Supported;
        private Collection<string> _allowedOrderByProperties = new Collection<string>();
        private int? _maxSkip;
        private int? _maxTop;
        private int _maxAnyAllExpressionDepth = 1;
        private int _maxNodeCount = 100;
        private int _maxExpansionDepth = DefaultMaxExpansionDepth;
        private int _maxOrderByNodeCount = 5;

        /// <summary>
        /// Gets or sets a list of allowed arithmetic operators including 'add', 'sub', 'mul', 'div', 'mod'.
        /// </summary>
        public AllowedArithmeticOperators AllowedArithmeticOperators
        {
            get
            {
                return _allowedArithmeticOperators;
            }
            set
            {
                if (value > AllowedArithmeticOperators.All || value < AllowedArithmeticOperators.None)
                {
                    throw Error.InvalidEnumArgument("value", (Int32)value, typeof(AllowedArithmeticOperators));
                }

                _allowedArithmeticOperators = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of allowed functions used in the $filter query. 
        /// 
        /// The allowed functions include the following:
        /// 
        /// String related: substringof, endswith, startswith, length, indexof, substring, tolower, toupper, trim, concat
        ///
        /// e.g. ~/Customers?$filter=length(CompanyName) eq 19
        ///
        /// DateTime related: year, years, month, months, day, days, hour, hours, minute, minutes, second, seconds
        ///
        /// e.g. ~/Employees?$filter=year(BirthDate) eq 1971
        ///
        /// Math related: round, floor, ceiling
        ///
        /// Type related:isof, cast, 
        ///
        /// Collection related: any, all
        ///  
        /// </summary>
        public AllowedFunctions AllowedFunctions
        {
            get
            {
                return _allowedFunctions;
            }
            set
            {
                if (value > AllowedFunctions.AllFunctions || value < AllowedFunctions.None)
                {
                    throw Error.InvalidEnumArgument("value", (Int32)value, typeof(AllowedFunctions));
                }

                _allowedFunctions = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of allowed logical operators such as 'eq', 'ne', 'gt', 'ge', 'lt', 'le', 'and', 'or', 'not'.
        /// </summary>
        public AllowedLogicalOperators AllowedLogicalOperators
        {
            get
            {
                return _allowedLogicalOperators;
            }
            set
            {
                if (value > AllowedLogicalOperators.All || value < AllowedLogicalOperators.None)
                {
                    throw Error.InvalidEnumArgument("value", (Int32)value, typeof(AllowedLogicalOperators));
                }

                _allowedLogicalOperators = value;
            }
        }

        /// <summary>
        /// Gets a list of properties one can orderby the result with. Note, by default this list is empty, 
        /// it means it can be ordered by any property.
        /// 
        /// For example, having an empty collection means client can order the queryable result by any properties.  
        /// Adding "Name" to this list means that it only allows queryable result to be ordered by Name property.
        /// </summary>
        public Collection<string> AllowedOrderByProperties
        {
            get
            {
                return _allowedOrderByProperties;
            }
        }

        /// <summary>
        /// Gets or sets the query parameters that are allowed inside query. The default is all query options,
        /// including $filter, $skip, $top, $orderby, $expand, $select, $count, $format and $skiptoken.
        /// </summary>
        public AllowedQueryOptions AllowedQueryOptions
        {
            get
            {
                return _allowedQueryParameters;
            }
            set
            {
                if (value > AllowedQueryOptions.All || value < AllowedQueryOptions.None)
                {
                    throw Error.InvalidEnumArgument("value", (Int32)value, typeof(AllowedQueryOptions));
                }

                _allowedQueryParameters = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of expressions that can be present in the $orderby.
        /// </summary>
        public int MaxOrderByNodeCount
        {
            get
            {
                return _maxOrderByNodeCount;
            }
            set
            {
                if (value < MinMaxOrderByNodeCount)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxOrderByNodeCount);
                }

                _maxOrderByNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query.
        /// </summary>
        /// <remarks>
        /// The default value is 1.
        /// </remarks>
        /// <value>
        /// The maximum depth of the Any or All elements nested inside the query.
        /// </value>
        public int MaxAnyAllExpressionDepth
        {
            get
            {
                return _maxAnyAllExpressionDepth;
            }
            set
            {
                if (value < MinMaxAnyAllExpressionDepth)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxAnyAllExpressionDepth);
                }

                _maxAnyAllExpressionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of the nodes inside the $filter syntax tree.
        /// </summary>
        /// <remarks>
        /// The default value is 100.
        /// </remarks>
        public int MaxNodeCount
        {
            get
            {
                return _maxNodeCount;
            }
            set
            {
                if (value < MinMaxNodeCount)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxNodeCount);
                }

                _maxNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $skip that a client can request.
        /// </summary>
        public int? MaxSkip
        {
            get
            {
                return _maxSkip;
            }
            set
            {
                if (value.HasValue && value < MinMaxSkip)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxSkip);
                }

                _maxSkip = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        public int? MaxTop
        {
            get
            {
                return _maxTop;
            }
            set
            {
                if (value.HasValue && value < MinMaxTop)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxTop);
                }

                _maxTop = value;
            }
        }

        /// <summary>
        /// Gets or sets the max expansion depth for the $expand query option.
        /// </summary>
        /// <remarks>To disable the maximum expansion depth check, set this property to 0.</remarks>
        public int MaxExpansionDepth
        {
            get { return _maxExpansionDepth; }
            set
            {
                if (value < MinMaxExpansionDepth)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, MinMaxExpansionDepth);
                }
                _maxExpansionDepth = value;
            }
        }
    }
}
