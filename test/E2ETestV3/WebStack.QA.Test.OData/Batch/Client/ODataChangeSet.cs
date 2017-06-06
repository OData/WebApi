using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebStack.QA.Test.OData.Batch.Client
{
    public class ODataChangeSet
    {
        private Queue<HttpRequestMessage> _messages = new Queue<HttpRequestMessage>();

        public IEnumerable<HttpRequestMessage> Requests
        {
            get
            {
                return _messages;
            }
        }

        private Lazy<string> _boundary = new Lazy<string>(() => string.Format("changeset_{0}", Guid.NewGuid().ToString()));

        public string Boundary
        {
            get { return _boundary.Value; }
        }


        public ODataChangeSet(IEnumerable<HttpRequestMessage> requests)
        {
            _messages = new Queue<HttpRequestMessage>(requests);
        }

        public void Add(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            _messages.Enqueue(request);
        }

        public HttpContent Build(string boundary)
        {
            if (boundary == null)
            {
                throw new ArgumentNullException("boundary");
            }
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new ArgumentException("The provided boundary value is invalid", "boundary");
            }
            MultipartContent content = new MultipartContent("mixed", boundary);
            foreach (var request in Requests)
            {
                HttpMessageContent messageContent = new HttpMessageContent(request);
                messageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
                messageContent.Headers.Add("Content-Transfer-Encoding", "binary");
                content.Add(messageContent);
            }
            return content;
        }

        public HttpContent Build()
        {
            return Build(Boundary);
        }
    }
}
