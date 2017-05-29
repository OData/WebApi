using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebStack.QA.Test.OData.Batch.Client
{
    public class ODataOperation
    {
        public HttpRequestMessage RawRequest { get; private set; }
        public ODataOperation(HttpRequestMessage request)
        {
            RawRequest = request;
        }

        public HttpContent Build()
        {
            HttpMessageContent messageContent = new HttpMessageContent(RawRequest);
            messageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
            messageContent.Headers.Add("Content-Transfer-Encoding", "binary");
            return messageContent;
        }
    }
}
