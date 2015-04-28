using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nuwa.Client
{
    /// <summary>
    /// ClientLogHandler log both request and response message to the console
    /// </summary>
    internal class ClientLogHandler : DelegatingHandler
    {
        public ClientLogHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();

            Console.WriteLine("Request {0}", request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine("Request Content: {0}", request.Content.ReadAsStringAsync().Result);
            }

            base.SendAsync(request, cancellationToken).ContinueWith(
                innerTask =>
                {
                    if (innerTask.IsCanceled)
                    {
                        Console.WriteLine("Task is cancelled, no response.");
                        tcs.SetCanceled();
                    }
                    else if (innerTask.IsFaulted)
                    {
                        Console.WriteLine("Task is faulted, no response.");
                        tcs.SetException(innerTask.Exception.InnerExceptions);
                    }
                    else if (innerTask.IsCompleted)
                    {
                        var response = innerTask.Result;
                        Console.WriteLine("Response {0}", response.ToString());
                        if (response.Content != null)
                        {
                            Console.WriteLine("Respose Content: {0}", response.Content.ReadAsStringAsync().Result);
                        }

                        tcs.SetResult(innerTask.Result);
                    }
                });

            return tcs.Task;
        }
    }
}
