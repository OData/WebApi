// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Exception type to indicate that json reader quotas have been exceeded.
    /// </summary>
    [Serializable]
    internal class JsonReaderQuotaException : JsonReaderException
    {
        public JsonReaderQuotaException()
            : base()
        {
        }

        public JsonReaderQuotaException(string message)
            : base(message)
        {
        }

        protected JsonReaderQuotaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public JsonReaderQuotaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
