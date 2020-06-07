// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from content type request headers.
    /// </summary>
    public partial class ContentTypeMediaTypeMapping : MediaTypeMapping
    {
        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;

            return contentType?.MediaType?.StartsWith(this.MediaType.MediaType, StringComparison.Ordinal) == true ? 1 : 0;
        }
    }
}
