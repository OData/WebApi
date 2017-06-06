using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebStack.QA.Test.OData.Batch.Client
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
