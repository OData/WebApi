// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData
{
    internal static class MediaTypeFormatterCollectionExtensions
    {
        public static ODataMediaTypeFormatter ODataFormatter(this MediaTypeFormatterCollection formatters)
        {
            Contract.Assert(formatters != null);

            // TODO 464640: this doesn't work when tracing is enabled.
            return formatters.OfType<ODataMediaTypeFormatter>().FirstOrDefault();
        }
    }
}
