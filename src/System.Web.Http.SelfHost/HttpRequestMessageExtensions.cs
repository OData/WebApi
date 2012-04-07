// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ServiceModel.Security;
using System.Web.Http;

namespace System.Net.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string SecurityKey = "Security";

        /// <summary>
        /// Gets the current <see cref="T:System.ServiceModel.Security.SecurityMessageProperty"/> 
        /// stored in <see cref="M:HttpRequestMessage.Properties"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="SecurityMessageProperty"/>.</returns>
        public static SecurityMessageProperty GetSecurityMessageProperty(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<SecurityMessageProperty>(SecurityKey);
        }

        private static T GetProperty<T>(this HttpRequestMessage request, string key)
        {
            T value;
            request.Properties.TryGetValue(key, out value);
            return value;
        }
    }
}
