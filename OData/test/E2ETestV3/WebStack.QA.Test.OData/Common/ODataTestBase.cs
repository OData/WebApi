using System;
using System.Net.Http;
using Nuwa;

namespace WebStack.QA.Test.OData.Common
{
    public interface IODataTestBase
    {
        string BaseAddress { get; set; }
        HttpClient Client { get; set; }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public abstract class ODataTestBase : IODataTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }
    }
}
