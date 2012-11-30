// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class MediaTypeFormatterExtensions
    {
        public static IEdmModel GetODataModel(this MediaTypeFormatter formatter)
        {
            IEdmModel model;
            IsODataFormatter(formatter, out model);
            return model;
        }

        public static bool IsODataFormatter(this MediaTypeFormatter formatter)
        {
            IEdmModel ignore;
            return IsODataFormatter(formatter, out ignore);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Calling the formatter only to identify the ODataFormatter; exceptions can be ignored")]
        private static bool IsODataFormatter(this MediaTypeFormatter formatter, out IEdmModel edmModel)
        {
            Contract.Assert(formatter != null);

            ODataMediaTypeFormatter odataFormatter = formatter as ODataMediaTypeFormatter;

            if (odataFormatter != null)
            {
                edmModel = odataFormatter.Model;
                return true;
            }

            // Detects ODataFormatters that are wrapped by tracing
            // Creates a dummy request message and sees if the formatter adds a model to the request properties
            // This is a workaround until tracing provides information about the wrapped inner formatter
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                try
                {
                    formatter.GetPerRequestFormatterInstance(typeof(IEdmModel), request, mediaType: null);
                    object model;

                    if (request.Properties.TryGetValue(ODataMediaTypeFormatter.EdmModelKey, out model))
                    {
                        edmModel = model as IEdmModel;

                        if (edmModel != null)
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // Ignore exceptions - it isn't the OData formatter we're looking for
                }
            }

            edmModel = null;
            return false;
        }
    }
}
