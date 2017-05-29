using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Nuwa;
using WebStack.QA.Common.WebHost;
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

            var selfhostConfig = configuration as HttpSelfHostConfiguration;
            if (selfhostConfig != null)
            {
                selfhostConfig.MaxReceivedMessageSize = selfhostConfig.MaxBufferSize = 5000000;
            }
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
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
                var error = response.Content.ReadAsAsync<HttpError>().Result;
                var message = (error["ExceptionMessage"] ?? string.Empty).ToString();

                Console.WriteLine("Message: " + message);

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
                else if (message.Contains("Exception has been thrown by the target of an invocation"))
                {
                    var innerMsg = (error["InnerException"] ?? string.Empty).ToString();
                    Console.WriteLine("InnerMessage : " + innerMsg);
                    if (innerMsg.Contains("Attempted to divide by zero"))
                    {
                        Assert.True(true);
                    }
                    else
                    {
                        Assert.False(true, string.Format("$filter:{0} \n Error message: {1}", filter, message));
                    }
                }
                else
                {
                    Assert.False(true, string.Format("$filter:{0} \n Error message: {1}", filter, message));
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

        [Theory]
        [PropertyData("FuzzingQueries")]
        public void TestFuzzingQueriesWithMultipleThreads(string filter)
        {
            Parallel.For(0, 3, i =>
            {
                TestFuzzingQueries(filter);
            });
        }
    }
}
