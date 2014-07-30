// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
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
        ///  Gets the raw $inlineCount query value from the incoming request Uri if exists.
        /// </summary>
        public string InlineCount { get; internal set; }

        /// <summary>
        ///  Gets the raw $format query value from the incoming request Uri if exists.
        /// </summary>
        public string Format { get; internal set; }

        /// <summary>
        ///  Gets the raw $skiptoken query value from the incoming request Uri if exists.
        /// </summary>
        public string SkipToken { get; internal set; }
    }
}
