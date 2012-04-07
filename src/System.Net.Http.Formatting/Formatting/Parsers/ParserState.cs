// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting.Parsers
{
    /// <summary>
    /// Represents the overall state of various parsers.
    /// </summary>
    internal enum ParserState
    {
        /// <summary>
        /// Need more data
        /// </summary>
        NeedMoreData = 0,

        /// <summary>
        /// Parsing completed (final)
        /// </summary>
        Done,

        /// <summary>
        /// Bad data format (final)
        /// </summary>
        Invalid,

        /// <summary>
        /// Data exceeds the allowed size (final)
        /// </summary>
        DataTooBig,
    }
}
