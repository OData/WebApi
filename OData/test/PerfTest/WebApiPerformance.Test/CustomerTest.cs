using System;
using System.Net;
using Microsoft.Xunit.Performance;
using Xunit;

namespace WebApiPerformance.Test
{
    public class CustomerTest : IClassFixture<TestServiceFixture>
    {
        private readonly TestServiceFixture serviceFixture;

        public CustomerTest(TestServiceFixture serviceFixture)
        {
            this.serviceFixture = serviceFixture;
        }

        [Benchmark]
        public void ODataClrTest()
        {
            Uri queryUri = new Uri(serviceFixture.ServiceBaseUri + "/ODataClr?n=1000");
            TestExecution(queryUri);
        }

        [Benchmark]
        public void ODataEdmTest()
        {
            Uri queryUri = new Uri(serviceFixture.ServiceBaseUri + "/ODataEdm?n=1000");
            TestExecution(queryUri);
        }

        [Benchmark]
        public void WebApiJsonTest()
        {
            Uri queryUri = new Uri(serviceFixture.ServiceBaseUri + "/api/WebApiJson?n=1000");
            TestExecution(queryUri);
        }

        private static void TestExecution(Uri queryUri)
        {
            Benchmark.Iterate(() => { new WebClient().DownloadData(queryUri); });
        }
    }
}
