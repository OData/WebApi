// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    internal static class MediaTypeHeaderValueExtensions
    {
        public static MediaTypeHeaderValue Clone(this MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null && mediaType.GetType() == typeof(MediaTypeHeaderValue));

            var result = new MediaTypeHeaderValue(mediaType.MediaType);
            foreach (var parameter in mediaType.Parameters)
            {
                result.Parameters.Add(new NameValueHeaderValue(parameter.Name, parameter.Value));
            }

            return result;
        }
    }
}
