// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Query
{
    /// <summary>
    /// OData query options to allow for querying.
    /// </summary>
    [Flags]
    public enum AllowedQueryOptions
    {
        /// <summary>
        /// A value that corresponds to allowing no query options.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A value that corresponds to allowing the $filter query option.
        /// </summary>
        Filter = 0x1,

        /// <summary>
        /// A value that corresponds to allowing the $expand query option.
        /// </summary>
        Expand = 0x2,

        /// <summary>
        /// A value that corresponds to allowing the $select query option.
        /// </summary>
        Select = 0x4,

        /// <summary>
        /// A value that corresponds to allowing the $orderby query option.
        /// </summary>
        OrderBy = 0x8,

        /// <summary>
        /// A value that corresponds to allowing the $top query option.
        /// </summary>
        Top = 0x10,

        /// <summary>
        /// A value that corresponds to allowing the $skip query option.
        /// </summary>
        Skip = 0x20,

        /// <summary>
        /// A value that corresponds to allowing the $count query option.
        /// </summary>
        Count = 0x40,

        /// <summary>
        /// A value that corresponds to allowing the $format query option.
        /// </summary>
        Format = 0x80,

        /// <summary>
        /// A value that corresponds to allowing the $skiptoken query option.
        /// </summary>
        SkipToken = 0x100,

        /// <summary>
        /// A value that corresponds to the default query options supported by <see cref="EnableQueryAttribute"/>.
        /// </summary>
        Supported = Filter | OrderBy | Top | Skip | Count | Select | Expand | Format,

        /// <summary>
        /// A value that corresponds to allowing all query options.
        /// </summary>
        All = Filter | Expand | Select | OrderBy | Top | Skip | Count | Format | SkipToken
    }
}
