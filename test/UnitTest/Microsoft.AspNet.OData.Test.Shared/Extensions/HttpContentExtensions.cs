//-----------------------------------------------------------------------------
// <copyright file="HttpContentExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using System.Net.Http;
using System.Threading.Tasks;
#endif

namespace Microsoft.AspNet.OData.Test.Extensions
{
    /// <summary>
    /// Extensions for HttpContent.
    /// </summary>
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Get the content as the value of ObjectContent.
        /// </summary>
        /// <returns>The content value.</returns>
        public static string AsObjectContentValue(this HttpContent content)
        {
#if NETCORE
            string json = content.ReadAsStringAsync().Result;
            try
            {
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                return obj["value"].ToString();
            }
            catch (JsonReaderException)
            {
            }

            return json;
#else
            return (content as ObjectContent<string>).Value as string;
#endif
        }

        /// <summary>
        /// A custom extension for AspNetCore to deserialize JSON content as an object.
        /// AspNet provides this in  System.Net.Http.Formatting.
        /// </summary>
        /// <returns>The content value.</returns>
        public static async Task<T> ReadAsObject<T>(this HttpContent content)
        {
#if NETCORE
            string json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
#else
            T obj = await content.ReadAsAsync<T>();
            return obj;
#endif
        }
    }
}
