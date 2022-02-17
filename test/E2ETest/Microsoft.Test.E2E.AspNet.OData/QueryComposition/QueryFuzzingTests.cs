//-----------------------------------------------------------------------------
// <copyright file="QueryFuzzingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.QueryComposition.Fuzzing;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class QueryFuzzingTests : WebHostTestBase
    {
        public QueryFuzzingTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string> FuzzingQueries
        {
            get
            {
                var data = new TheoryDataSet<string>();
                int seed = RandomSeedGenerator.GetRandomSeed();
                Trace.WriteLine($"Generated seed for random number generator: {seed}");

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

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
            configuration.MaxReceivedMessageSize = int.MaxValue;
        }

        [Theory]
        [MemberData(nameof(FuzzingQueries))]
        public async Task TestFuzzingQueries(string filter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/Fuzzing?$top=1&" + filter);
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
                var message = await response.Content.ReadAsStringAsync();
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
                else if (message.Contains("The query specified in the URI is not valid. "))
                {
                    // There are cases that duplicated query options can lead to message indicating invalid URI. For example:
                    // $filter=(ComplexTypeProperty/DateTimeOffsetProperty le SingleNavigationProperty/SingleNavigationProperty/DateTimeOffsetProperty)
                    // &$filter=(floor(round(123.123M) sub DecimalProperty mul DecimalProperty add DecimalProperty) le round(DecimalProperty) add 123.123M sub SingleNavigationProperty/DecimalProperty div ComplexTypeProperty/DecimalProperty sub DecimalProperty)"
                    Assert.True(true);
                }
                else
                {
                    Assert.False(true);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("The request filtering module is configured to deny a request where the query string is too long.", content);
            }
            else
            {
                Assert.False(true);
            }
        }
    }
}
