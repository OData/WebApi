// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.AspNet.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.QueryComposition.Fuzzing;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class QueryFuzzingTests : ODataTestBase
    {
        public static TheoryDataSet<string> FuzzingQueries
        {
            get
            {
                var data = new TheoryDataSet<string>();
                int seed = RandomSeedGenerator.GetRandomSeed();
                var ranGen = new Random(seed);

                for (int i = 5; i <= 10; i += 5)
                {
                    var settings = new CreatorSettings()
                    {
                        MaxGraphDepth = i
                    };
                    var syntax = QueryBuilder.CreateODataQuerySyntax();

                    for (int j = 0; j < 5; j++)
                    {
                        data.Add(syntax.Generate(ranGen, settings));
                    }
                }

                return data;
            }
        }

        public static TheoryDataSet<string> AdHocQueries
        {
            get
            {
                var data = new TheoryDataSet<string>();
                data.Add("$filter=(indexof('stringLiternal', ComplexTypeProperty/CharArrayProperty) gt SingleNavigationProperty/UInt64Property)");

                return data;
            }
        }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();

            var selfhostConfig = configuration as HttpSelfHostConfiguration;
            if (selfhostConfig != null)
            {
                selfhostConfig.MaxReceivedMessageSize = selfhostConfig.MaxBufferSize = 5000000;
            }
        }

        [Theory]
        [PropertyData("FuzzingQueries")]
        public void TestFuzzingQueries(string filter)
        {
            var handler = typeof(HttpMessageInvoker).GetField("handler", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this.Client) as HttpMessageHandler;
            this.Client = new HttpClient(handler);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var response = this.Client.GetAsync(this.BaseAddress + "/api/Fuzzing?$top=1&" + filter).Result;
            sw.Stop();
            Console.WriteLine("Time cost: " + sw.ElapsedMilliseconds);

            if (response.IsSuccessStatusCode)
            {
                // Pass if successful
                Assert.True(true);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                || response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var message = response.Content.ReadAsStringAsync().Result;
                if (message.Contains("The recursion limit has been exceeded"))
                {
                    // Pass if expected error occurs
                    Assert.True(true);
                }
                else if (message.Contains("Invalid URI: The Uri scheme is too long."))
                {
                    Assert.True(true);
                }
                else if (message.Contains("Duplicate property named"))
                {
                    Assert.True(true);
                }
                else if (message.Contains("Only ordering by properties at the root level is supported for non-primitive collections. Nested properties and expressions are not supported."))
                {
                    Assert.True(true);
                }
                else if (message.Contains("System.DivideByZeroException"))
                {
                    Assert.True(true);
                }
                else if (message.Contains("An item with the same key has already been added"))
                {
                    // There are duplicate query options, for example, there are more than one $filter, such as:
                    // GET /api/Fuzzing?$top=1&$filter=(length(ComplexTypeProperty/StringProperty)%20eq%20indexof(SingleNavigationProperty/SingleNavigationProperty/StringProperty,%20ComplexTypeProperty/StringProperty))
                    // &$filter=(length(tolower(tolower(toupper(ComplexTypeProperty/StringProperty))))%20eq%20ComplexTypeProperty/Int32Property)
                    // &$filter=(indexof(toupper(trim(toupper(tolower('stringLiternal')))),%20toupper(trim(SingleNavigationProperty/SingleNavigationProperty/StringProperty)))%20eq%20indexof(concat(ComplexTypeProperty/StringProperty,%20'stringLiternal'),%20StringProperty))
                    // &$filter=(trim(tolower(trim(ComplexTypeProperty/StringProperty)))%20ne%20tolower('stringLiternal'))
                    // &$filter=(indexof(tolower(StringProperty),%20trim(ComplexTypeProperty/StringProperty))%20eq%20indexof('stringLiternal',%20tolower(trim(StringProperty))))
                    // &$filter=(month(2012-09-11T00:09:00%2B08:00)%20ne%20indexof(tolower(tolower(StringProperty)),%20ComplexTypeProperty/StringProperty))
                    // &$filter=(indexof(trim('stringLiternal'),%20tolower(toupper(StringProperty)))%20eq%20indexof('stringLiternal',%20tolower(ComplexTypeProperty/StringProperty)))
                    Assert.True(true);
                }
                else
                {
                    Assert.False(true);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Assert.Contains("The request filtering module is configured to deny a request where the query string is too long.", content);
            }
            else
            {
                Assert.False(true);
            }
        }

        //[Theory(Skip = "It is not stable, now disable it to prevent it from hiding other test failures.")]
        //[PropertyData("FuzzingQueries")]
        public void TestFuzzingQueriesWithMultipleThreads(string filter)
        {
            Parallel.For(0, 3, i =>
            {
                TestFuzzingQueries(filter);
            });
        }
    }
}
