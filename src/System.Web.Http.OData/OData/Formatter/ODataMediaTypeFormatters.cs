// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataMediaTypeFormatters
    {
        /// <summary>
        /// Creates a set of media type formatters to handle OData.
        /// </summary>
        /// <param name="model">The data model the formatter will support.</param>
        /// <returns>A set of media type formatters to handle OData.</returns>
        public static IEnumerable<ODataMediaTypeFormatter> Create(IEdmModel model)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(model);

            formatter.SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationAtomXmlMediaType);
            formatter.SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationJsonMediaType);
            formatter.SupportedMediaTypes.Add(ODataFormatterConstants.ApplicationXmlMediaType);

            formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true,
                throwOnInvalidBytes: true));
            formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true));

            return new ODataMediaTypeFormatter[] { formatter };
        }
    }
}
