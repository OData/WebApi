//-----------------------------------------------------------------------------
// <copyright file="CustomerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        [MeasureGCAllocations]
        public void ODataClrTest()
        {
            Uri queryUri = new Uri(serviceFixture.ServiceBaseUri + "/ODataClr?n=1000");
            TestExecution(queryUri);
        }

        [Benchmark]
        [MeasureGCAllocations]
        public void ODataEdmTest()
        {
            Uri queryUri = new Uri(serviceFixture.ServiceBaseUri + "/ODataEdm?n=1000");
            TestExecution(queryUri);
        }

        [Benchmark]
        [MeasureGCAllocations]
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
