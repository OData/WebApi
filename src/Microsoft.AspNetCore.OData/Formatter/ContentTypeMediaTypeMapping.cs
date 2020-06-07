// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from content type request headers.
    /// </summary>
    public partial class ContentTypeMediaTypeMapping : MediaTypeMapping
    {
        /// <inheritdoc />
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ContentType?.StartsWith(this.MediaType.MediaType, StringComparison.Ordinal) == true ? 1 : 0;
        }
    }
}
