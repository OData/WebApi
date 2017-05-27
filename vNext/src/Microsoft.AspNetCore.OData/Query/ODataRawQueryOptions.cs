// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents the raw query values in the string format from the incoming request.
    /// </summary>
    public class ODataRawQueryOptions
    {
        /// <summary>
        ///  Gets the raw $filter query value from the incoming request Uri if exists.
        /// </summary>
        public string Filter { get; internal set; }

        /// <summary>
        ///  Gets the raw $orderby query value from the incoming request Uri if exists.
        /// </summary>
        public string OrderBy { get; internal set; }

        /// <summary>
        ///  Gets the raw $top query value from the incoming request Uri if exists.
        /// </summary>
        public string Top { get; internal set; }

        /// <summary>
        ///  Gets the raw $skip query value from the incoming request Uri if exists.
        /// </summary>
        public string Skip { get; internal set; }

        /// <summary>
        ///  Gets the raw $select query value from the incoming request Uri if exists.
        /// </summary>
        public string Select { get; internal set; }

        /// <summary>
        ///  Gets the raw $expand query value from the incoming request Uri if exists.
        /// </summary>
        public string Expand { get; internal set; }

        /// <summary>
        ///  Gets the raw $count query value from the incoming request Uri if exists.
        /// </summary>
        public string Count { get; internal set; }

        /// <summary>
        ///  Gets the raw $format query value from the incoming request Uri if exists.
        /// </summary>
        public string Format { get; internal set; }

        /// <summary>
        ///  Gets the raw $skiptoken query value from the incoming request Uri if exists.
        /// </summary>
        public string SkipToken { get; internal set; }

        /// <summary>
        ///  Gets the raw $deltatoken query value from the incoming request Uri if exists.
        /// </summary>
        public string DeltaToken { get; internal set; }
    }
}
