// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Batch.Client
{
    public class JsonLightContent : HttpContent
    {
        public Uri ODataMetadata { get; set; }
        public string ODataType { get; set; }
        public object Payload { get; set; }

        public JsonLightContent(object payload)
        {
            Payload = payload;
        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Contract.Assert(stream != null, "stream can't be null");
            Contract.Assert(context != null, "context can't be null");
            JObject json = (JObject)JToken.FromObject(Payload);
            if (ODataMetadata != null)
            {
                json.Add("odata.metadata", ODataMetadata);
            }
            if (ODataType != null)
            {
                json.Add("odata.type", ODataType);
            }
            return Task.Factory.StartNew(() =>
            {
                JsonTextWriter writer = new JsonTextWriter(new StreamWriter(stream));
                writer.Formatting = Formatting.Indented;
                json.WriteTo(writer);
                writer.Flush();
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
