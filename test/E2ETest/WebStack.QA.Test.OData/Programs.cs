// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.Http.SelfHost;
using WebStack.QA.Test.OData.QueryComposition;

namespace WebStack.QA.Test.OData
{
    public class Programs
    {
        public static void Main()
        {
            string baseAddress = "http://localhost:9022";
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { action = RouteParameter.Optional, id = RouteParameter.Optional });
            SecurityTests.UpdateConfiguration(config);

            HttpSelfHostServer server = new HttpSelfHostServer(config);
            server.OpenAsync();

            SecurityTests tests = new SecurityTests();
            tests.BaseAddress = baseAddress;
            tests.Client = new System.Net.Http.HttpClient();

            foreach (var data in SecurityTests.DoSAttackData)
            {
                tests.TestDosAttackWithMultipleThreads(data[0] as string);
            }
        }
    }
}
