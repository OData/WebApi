// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http.WebHost
{
    public class RemoteHttpControllerHandlerTest : MarshalByRefObject
    {
        public ConvertRequest_DoesLazyGetBufferlessInputStream_TestResults ConvertRequest_DoesLazyGetBufferlessInputStream()
        {
            bool inputStreamCalled = false;

            HttpRequestBase stubRequest = HttpControllerHandlerTest.CreateStubRequestBase(() =>
            {
                inputStreamCalled = true;
                return new MemoryStream();
            },
            buffered: false);
            HttpContextBase context = HttpControllerHandlerTest.CreateStubContextBase(request: stubRequest, items: null);

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHostBufferPolicySelector), new BufferOutputOnlyPolicySelector());
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(context);

            ConvertRequest_DoesLazyGetBufferlessInputStream_TestResults results = new ConvertRequest_DoesLazyGetBufferlessInputStream_TestResults();
            results.inputStreamCalledBeforeContentIsRead = inputStreamCalled;
            Stream contentStream = actualRequest.Content.ReadAsStreamAsync().Result;
            results.inputStreamCalledAfterContentIsRead = inputStreamCalled;
            return results;
        }

        private class BufferOutputOnlyPolicySelector : IHostBufferPolicySelector
        {
            public bool UseBufferedInputStream(object hostContext)
            {
                return false;
            }

            public bool UseBufferedOutputStream(HttpResponseMessage response)
            {
                return true;
            }
        }
    }

    [Serializable]
    public class ConvertRequest_DoesLazyGetBufferlessInputStream_TestResults
    {
        public bool inputStreamCalledBeforeContentIsRead { get; set; }

        public bool inputStreamCalledAfterContentIsRead { get; set; }
    }
}