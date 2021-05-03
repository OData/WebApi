// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DateAndTimeOfDay
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
    public class DateAndTimeOfDayTest : WebHostTestBase
    {
        public DateAndTimeOfDayTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(DCustomersController), typeof(MetadataController), typeof(EfCustomersController) };
            configuration.AddControllers(controllers);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); // -8:00
            configuration.SetTimeZoneInfo(timeZoneInfo);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute(
                routeName: "convention",
                routePrefix: "convention",
                model: DateAndTimeOfDayEdmModel.GetConventionModel(configuration));

            configuration.MapODataServiceRoute(
                routeName: "explicit",
                routePrefix: "explicit",
                model: DateAndTimeOfDayEdmModel.GetExplicitModel(),
                batchHandler: configuration.CreateDefaultODataBatchHandler());

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("convention")]
        [InlineData("explicit")]
        public async Task ModelBuilderTest(string modelMode)
        {
            string requestUri = string.Format("{0}/{1}/$metadata", this.BaseAddress, modelMode);

            HttpResponseMessage response = await this.Client.GetAsync(requestUri);
            var stream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage message = new ODataMessageWrapper(stream, response.Content.Headers);
            var reader = new ODataMessageReader(message);
            var edmModel = reader.ReadMetadataDocument();

            var customerType = edmModel.SchemaElements.OfType<IEdmEntityType>().Single(et => et.Name == "DCustomer");
            Assert.Equal(17, customerType.Properties().Count());

            // Non-Nullable
            AssertHasProperty(customerType, propertyName: "DateTime", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.DateTimeOffset", isNullable: false);
            AssertHasProperty(customerType, propertyName: "Offset", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.DateTimeOffset", isNullable: false);
            AssertHasProperty(customerType, propertyName: "Date", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.Date", isNullable: false);
            AssertHasProperty(customerType, propertyName: "TimeOfDay", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.TimeOfDay", isNullable: false);

            // Nullable
            AssertHasProperty(customerType, propertyName: "NullableDateTime", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.DateTimeOffset", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableOffset", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.DateTimeOffset", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableDate", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.Date", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableTimeOfDay", expectKind: EdmTypeKind.Primitive, expectTypeName: "Edm.TimeOfDay", isNullable: true);

            // Collection
            AssertHasProperty(customerType, propertyName: "DateTimes", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.DateTimeOffset)", isNullable: false);
            AssertHasProperty(customerType, propertyName: "Offsets", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.DateTimeOffset)", isNullable: false);
            AssertHasProperty(customerType, propertyName: "Dates", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.Date)", isNullable: false);
            AssertHasProperty(customerType, propertyName: "TimeOfDays", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.TimeOfDay)", isNullable: false);

            // nullable collection
            AssertHasProperty(customerType, propertyName: "NullableDateTimes", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.DateTimeOffset)", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableOffsets", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.DateTimeOffset)", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableDates", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.Date)", isNullable: true);
            AssertHasProperty(customerType, propertyName: "NullableTimeOfDays", expectKind: EdmTypeKind.Collection, expectTypeName: "Collection(Edm.TimeOfDay)", isNullable: true);
        }

        public static TheoryDataSet<string, string> MediaTypes
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] mimes = new string[]{
                    "json",
                    "application/json",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full"};
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string mime in mimes)
                    {
                        data.Add(mode, mime);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(MediaTypes))]
        public async Task QueryDCustomerEntityTest(string mode, string mime)
        {
            string requestUri = string.Format("{0}/{1}/DCustomers(2)?$format={2}", BaseAddress, mode, mime);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, content["Id"]);
            Assert.Equal(DateTimeOffset.Parse("2017-01-01T17:02:03.004+08:00"), content["DateTime"]);
            Assert.Equal(DateTimeOffset.Parse("2015-03-01T01:02:03.004Z"), content["Offset"]);
            Assert.Equal("2015-01-03", content["Date"]);
            Assert.Equal("23:02:03.0140000", content["TimeOfDay"]);

            Assert.Null((DateTimeOffset?)(content["NullableDateTime"]));
            JsonAssert.PropertyEquals((Date?)null, "NullableDate", content);
        }

        public static TheoryDataSet<string, string, IList<int>> FilterData
        {
            get
            {
                string[] modes = { "convention", "explicit" };
                object[][] orders = {
                    // DateTime
                    new object[] {"$filter=DateTime eq cast(2016-01-01T17:02:03.004%2B08:00,Edm.DateTimeOffset)", new[] {1} },
                    new object[] {"$filter=DateTime ge cast(2016-01-01T17:02:03.004%2B08:00,Edm.DateTimeOffset)", new[] {1,2,3,4,5} },
                    new object[] {"$filter=DateTime lt cast(2016-01-01T17:02:03.004%2B08:00,Edm.DateTimeOffset)", new int[] {} },
                    new object[] {"$filter=DateTime ge cast(2016-01-01T17:03:03.004%2B08:00,Edm.DateTimeOffset)", new[] {2,3,4,5} },

                    // DateTimeOffset
                    new object[] {"$filter=Offset eq cast(2015-05-01T01:02:03.004Z,Edm.DateTimeOffset)", new[] {4} },
                    new object[] {"$filter=Offset ge cast(2015-05-01T01:02:03.004Z,Edm.DateTimeOffset)", new[] {4} },
                    new object[] {"$filter=Offset lt cast(2015-05-01T01:02:03.004Z,Edm.DateTimeOffset)", new int[] {1,2,3,5} },

                    // Date
                    new object[] {"$filter=Date eq 2014-12-29", new[] {3} },
                    new object[] {"$filter=Date ge 2014-12-29", new[] {1,2,3,4} },
                    new object[] {"$filter=Date lt 2014-12-29", new int[] {5} },

                    // TimeOfDay
                    new object[] {"$filter=TimeOfDay eq 21:02:03.0140000", new[] {4} },
                    new object[] {"$filter=TimeOfDay ge 21:02:03.0040000", new[] {2,4} },
                    new object[] {"$filter=TimeOfDay lt 21:02:03.0040000", new int[] {1,3,5} },

                    // DateTime?
                    new object[] {"$filter=NullableDateTime eq null", new[] {2,4} },
                    new object[] {"$filter=NullableDateTime ne null", new[] {1,3,5} },
                    new object[] {"$filter=NullableDateTime lt cast(2016-01-01T17:02:03.004%2B08:00,Edm.DateTimeOffset)", new int[] {} },

                    // DateTimeOffset?
                    new object[] {"$filter=NullableOffset eq null", new[] {3} },
                    new object[] {"$filter=NullableOffset ne null", new[] {1,2,4,5} },
                    new object[] {"$filter=NullableOffset lt cast(2015-05-01T01:02:03.004Z,Edm.DateTimeOffset)", new [] {1,2} },

                    // Date?
                    new object[] {"$filter=NullableDate eq null", new[] {2,4} },
                    new object[] {"$filter=NullableDate ne null", new[] {1,3,5} },
                    new object[] {"$filter=NullableDate lt 2015-01-03", new [] {1} },

                    // TimeOfDay?
                    new object[] {"$filter=NullableTimeOfDay eq null", new[] {3} },
                    new object[] {"$filter=NullableTimeOfDay ne null", new[] {1,2,4,5} },
                    new object[] {"$filter=NullableTimeOfDay gt 03:02:03.0040000", new [] {4,5} },

                    // fractionalseconds()
                    new object[] {"$filter=fractionalseconds(DateTime) eq 0.004", new[] {1,2,3,4,5} },
                    new object[] {"$filter=fractionalseconds(Offset) gt 0.004", new[] {1,3,5} },
                    new object[] {"$filter=fractionalseconds(TimeOfDay) lt 0.014", new [] {3} },

                    new object[] {"$filter=fractionalseconds(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=fractionalseconds(NullableOffset) eq 0.004", new[] {1,2,4,5} },
                    new object[] {"$filter=fractionalseconds(NullableTimeOfDay) ne 0.004", new int[] {} },

                    // date()
                    new object[] {"$filter=date(DateTime) eq 2016-01-01", new[] {1} },
                    new object[] {"$filter=date(Offset) gt 2015-01-02", new[] {2,3,4,5} },

                    new object[] {"$filter=date(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=date(NullableOffset) eq 2015-03-01", new[] {2} },

                    // time()
                    new object[] {"$filter=time(DateTime) eq 01:02:03.004", new[] {1,2,3,4,5} },
                    new object[] {"$filter=time(Offset) lt 01:02:03.014", new[] {2,4} },

                    new object[] {"$filter=time(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=time(NullableOffset) ne null", new[] {1,2,4,5} }
                };
                TheoryDataSet<string, string, IList<int>> data = new TheoryDataSet<string, string, IList<int>>();
                foreach (string mode in modes)
                {
                    foreach (object[] order in orders)
                    {
                        data.Add(mode, order[0] as string, order[1] as IList<int>);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(FilterData))]
        public async Task CanFilterDateAndTimeOfDayProperty(string mode, string filter, IList<int> expect)
        {
            string requestUri = string.Format("{0}/{1}/DCustomers?{2}", BaseAddress, mode, filter);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(expect.Count, content["value"].Count());

            IList<int> actual = new List<int>();
            for (int i = 0; i < expect.Count; i++)
            {
                actual.Add((int)content["value"][i]["Id"]);
            }

            Assert.Equal(expect, actual.ToArray());
        }

        public static TheoryDataSet<string, string, string> OrderByData
        {
            get
            {
                string[] modes = { "convention", "explicit" };
                string[][] orders = {
                    new[] {"$orderby=DateTime", "1 > 2 > 3 > 4 > 5"},
                    new[] {"$orderby=DateTime desc", "5 > 4 > 3 > 2 > 1"},

                    new[] {"$orderby=Offset", "1 > 3 > 5 > 2 > 4"},
                    new[] {"$orderby=Offset desc", "4 > 2 > 5 > 3 > 1"},

                    new[] {"$orderby=Date", "5 > 3 > 1 > 2 > 4"},
                    new[] {"$orderby=Date desc", "4 > 2 > 1 > 3 > 5"},

                    new[] {"$orderby=TimeOfDay", "1 > 3 > 5 > 4 > 2"},
                    new[] {"$orderby=TimeOfDay desc", "2 > 4 > 5 > 3 > 1"},

                    new[] {"$orderby=NullableDateTime", "2 > 4 > 1 > 3 > 5"}, // Make sure 2 > 4, not 4 > 2
                    new[] {"$orderby=NullableDateTime desc", "5 > 3 > 1 > 2 > 4"},

                    new[] {"$orderby=NullableOffset", "3 > 1 > 2 > 4 > 5"},
                    new[] {"$orderby=NullableOffset desc", "5 > 4 > 2 > 1 > 3"},

                    new[] {"$orderby=NullableDate", "2 > 4 > 1 > 3 > 5"}, // Make sure 2 > 4, not 4 > 2
                    new[] {"$orderby=NullableDate desc", "5 > 3 > 1 > 2 > 4"},

                    new[] {"$orderby=NullableTimeOfDay", "3 > 1 > 2 > 4 > 5"},
                    new[] {"$orderby=NullableTimeOfDay desc", "5 > 4 > 2 > 1 > 3"},
                };
                TheoryDataSet<string, string, string> data = new TheoryDataSet<string, string, string>();
                foreach (string mode in modes)
                {
                    foreach (string[] order in orders)
                    {
                        data.Add(mode, order[0], order[1]);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(OrderByData))]
        public async Task CanOrderByDateAndTimeOfDayProperty(string mode, string orderby, string expect)
        {
            string requestUri = string.Format("{0}/{1}/DCustomers?{2}", BaseAddress, mode, orderby);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(5, content["value"].Count());

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(content["value"][i]["Id"]).Append(" > ");
            }
            sb.Remove(sb.Length - 3, 3); // remove the last " > "

            Assert.Equal(expect, sb.ToString());
        }

        #region function/action on Date & TimeOfDay

        public static TheoryDataSet<string, string> FunctionData
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] functions = new string[]
                {
                    "DCustomers(2)/Default.BoundFunction",
                    "UnboundFunction",
                };
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string f in functions)
                    {
                        data.Add(mode, f);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(FunctionData))]
        public async Task CanCallFunctionWithDateAndTimeOfDayParameters(string mode, string function)
        {
            string parameter =
                "modifiedDate=2015-02-28,modifiedTime=01:02:03.0040000,nullableModifiedDate=null,nullableModifiedTime=null";
            string requestUri = string.Format("{0}/{1}/{2}({3})", BaseAddress, mode, function, parameter);

            HttpResponseMessage response = await Client.GetAsync(requestUri);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(
                "modifiedDate:2015-02-28,modifiedTime:01:02:03.0040000,nullableModifiedDate:null,nullableModifiedTime:null",
                content["value"]);
        }

        public static TheoryDataSet<string, string> ActionData
        {
            get
            {
                string[] modes = new string[] { "convention", "explicit" };
                string[] actions = new string[]
                {
                    "DCustomers(2)/Default.BoundAction",
                    "UnboundAction",
                };
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string mode in modes)
                {
                    foreach (string action in actions)
                    {
                        data.Add(mode, action);
                    }
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ActionData))]
        public async Task CanCallActionWithDateAndTimeOfDayParameters(string mode, string action)
        {
            string requestUri = string.Format("{0}/{1}/{2}", BaseAddress, mode, action);
            string content = "{'modifiedDate':'2015-03-01','modifiedTime':'01:05:06.0080000','nullableModifiedDate':null,'nullableModifiedTime':null,'dates':['2014-12-21','2015-03-01']}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(content)
            };

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"value\":true", await response.Content.ReadAsStringAsync());
        }
        #endregion

        private static void AssertHasProperty(IEdmEntityType entityType, string propertyName, EdmTypeKind expectKind,
            string expectTypeName, bool isNullable)
        {
            Assert.NotNull(entityType);
            var property = entityType.DeclaredProperties.Single(p => p.Name == propertyName);
            Assert.Equal(expectKind, property.Type.TypeKind());
            Assert.Equal(expectTypeName, property.Type.Definition.FullTypeName());
            Assert.Equal(isNullable, property.Type.IsNullable);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task QueryEfCustomerEntityTest(string mime)
        {
            await ResetDatasource("convention");

            string requestUri = string.Format("{0}/convention/EfCustomers(2)?$format={1}", BaseAddress, mime);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(2, content["Id"]);
            Assert.Equal(DateTimeOffset.Parse("2016-12-24T03:02:03.006-08:00"), content["DateTime"]);
            Assert.Equal(DateTimeOffset.Parse("2015-02-24T03:02:03.006-08:00"), content["Offset"]);
            Assert.Equal(DateTimeOffset.Parse("2014-12-26T11:02:03.004-08:00"), content["NullableOffset"]);

            Assert.Null((DateTimeOffset?)(content["NullableDateTime"]));
        }

        public static TheoryDataSet<string, IList<int>> FilterDataForEf
        {
            get
            {
                object[][] orders = {
                    // DateTime
                    new object[] {"$filter=DateTime eq cast(2017-12-24T12:02:03.007Z,Edm.DateTimeOffset)", new[] {3} },
                    new object[] {"$filter=DateTime ge cast(2017-12-24T12:02:03.007Z,Edm.DateTimeOffset)", new[] {3,4,5} },
                    new object[] {"$filter=DateTime lt cast(2017-12-24T12:02:03.007Z,Edm.DateTimeOffset)", new[] {1,2} },

                    // DateTimeOffset
                    new object[] {"$filter=Offset eq cast(2015-04-24T13:02:03.008Z,Edm.DateTimeOffset)", new[] {4} },
                    new object[] {"$filter=Offset ge cast(2015-04-24T13:02:03.008Z,Edm.DateTimeOffset)", new[] {4,5} },
                    new object[] {"$filter=Offset lt cast(2015-04-24T13:02:03.008Z,Edm.DateTimeOffset)", new int[] {1,2,3} },

                    // DateTime?
                    new object[] {"$filter=NullableDateTime eq null", new[] {2,4} },
                    new object[] {"$filter=NullableDateTime ne null", new[] {1,3,5} },
                    new object[] {"$filter=NullableDateTime lt cast(2016-01-01T17:02:03.004%2B08:00,Edm.DateTimeOffset)", new[] {1,3,5} },

                    // DateTimeOffset?
                    new object[] {"$filter=NullableOffset eq null", new[] {3} },
                    new object[] {"$filter=NullableOffset ne null", new[] {1,2,4,5} },
                    new object[] {"$filter=NullableOffset lt cast(2014-12-29T01:02:03.004Z,Edm.DateTimeOffset)", new [] {1,2} },

                    // fractionalseconds()
                    new object[] {"$filter=fractionalseconds(DateTime) eq 0.007", new[] {3} },
                    new object[] {"$filter=fractionalseconds(Offset) gt 0.004", new[] {1,2,3,4,5} },

                    new object[] {"$filter=fractionalseconds(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=fractionalseconds(NullableOffset) lt 0.004", new int[] {} },

                    // date(DateTime)
                    new object[] {"$filter=date(DateTime) eq 2017-12-24", new[] {3} },
                    new object[] {"$filter=2017-12-24 eq date(DateTime)", new[] {3} },
                    new object[] {"$filter=date(DateTime) lt 2017-12-24", new[] {1,2} },
                    new object[] {"$filter=2017-12-24 le date(DateTime)", new[] {3,4,5 } },

                    // date(DateTimeOffset)
                    new object[] {"$filter=date(Offset) ne 2015-03-24", new[] {1,2,4,5} },
                    new object[] {"$filter=2015-03-24 eq date(Offset)", new[] {3} },
                    new object[] {"$filter=date(Offset) lt 2015-02-24", new[] {1} },
                    new object[] {"$filter=2015-02-24 le date(Offset)", new[] {2,3,4,5} },

                    // date(DateTime?)
                    new object[] {"$filter=date(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=null ne date(NullableDateTime)", new[] {1,3,5} },
                    new object[] {"$filter=date(NullableDateTime) eq 2014-12-24", new[] {1,3} }, // vary with the time zone setting.
                    new object[] {"$filter=date(NullableDateTime) gt 2014-12-24", new[] {5} }, // vary with the time zone setting.
                    new object[] {"$filter=date(NullableDateTime) lt 2014-12-24", new int[] {} },

                    // date(DateTimeOffset?)
                    new object[] {"$filter=date(NullableOffset) eq null", new[] {3} },
                    new object[] {"$filter=null ne date(NullableOffset)", new[] {1,2,4,5} },
                    new object[] {"$filter=date(NullableOffset) eq 2014-12-26", new[] {2} },
                    new object[] {"$filter=2014-12-28 ne date(NullableOffset)", new[] {1,2,3,5} },

                    // time(DateTime)
                    new object[] {"$filter=time(DateTime) eq 02:02:03.005", new[] {1} },
                    new object[] {"$filter=05:02:03.008 eq time(DateTime)", new[] {4} },
                    new object[] {"$filter=time(DateTime) lt 05:02:03.008", new[] {1,2,3} },

                    // time(DateTimeOffset)
                    new object[] {"$filter=time(Offset) eq 02:02:03.005", new[] {1} },
                    new object[] {"$filter=05:02:03.008 eq time(Offset)", new[] {4} },
                    new object[] {"$filter=time(Offset) lt 03:02:03.007", new[] {1,2} },

                    // time(DateTime?)
                    new object[] {"$filter=time(NullableDateTime) eq null", new[] {2,4} },
                    new object[] {"$filter=null ne time(NullableDateTime)", new[] {1,3,5} },
                    new object[] {"$filter=time(NullableDateTime) lt 06:02:04.005", new[] {1,5} },

                    // time(DateTimeOffset?)
                    new object[] {"$filter=time(NullableOffset) eq null", new[] {3} },
                    new object[] {"$filter=null ne time(NullableOffset)", new[] {1,2,4,5} },
                    new object[] {"$filter=time(NullableOffset) eq 21:02:03.004", new[] {4} },
                    new object[] {"$filter=21:02:03.004 ne time(NullableOffset)", new[] {1,2,3,5} },
                };
                TheoryDataSet<string, IList<int>> data = new TheoryDataSet<string, IList<int>>();
                foreach (object[] order in orders)
                {
                    data.Add(order[0] as string, order[1] as IList<int>);
                }

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(FilterDataForEf))]
        public async Task CanFilterDateAndTimeOfDayPropertyOnEf(string filter, IList<int> expect)
        {
            await ResetDatasource("convention");

            string requestUri = string.Format("{0}/convention/EfCustomers?{1}", BaseAddress, filter);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(expect.Count, content["value"].Count());

            IList<int> actual = new List<int>();
            for (int i = 0; i < expect.Count; i++)
            {
                actual.Add((int)content["value"][i]["Id"]);
            }

            Assert.Equal(expect, actual.ToArray());
        }

        public static TheoryDataSet<string, string> OrderByDataEf
        {
            get
            {
                string[][] orders = {
                    new[] {"$orderby=DateTime", "1 > 2 > 3 > 4 > 5"},
                    new[] {"$orderby=DateTime desc", "5 > 4 > 3 > 2 > 1"},

                    new[] {"$orderby=Offset", "1 > 2 > 3 > 4 > 5"},
                    new[] {"$orderby=Offset desc", "5 > 4 > 3 > 2 > 1"},

                    new[] {"$orderby=NullableDateTime", "2 > 4 > 1 > 3 > 5"}, // Make sure 2 > 4, not 4 > 2
                    new[] {"$orderby=NullableDateTime desc", "5 > 3 > 1 > 2 > 4"},

                    new[] {"$orderby=NullableOffset", "3 > 1 > 2 > 4 > 5"},
                    new[] {"$orderby=NullableOffset desc", "5 > 4 > 2 > 1 > 3"},
                };
                TheoryDataSet<string, string> data = new TheoryDataSet<string, string>();
                foreach (string[] order in orders)
                {
                    data.Add(order[0], order[1]);
                }
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(OrderByDataEf))]
        public async Task CanOrderByDateAndTimeOfDayPropertyOnEf(string orderby, string expect)
        {
            await ResetDatasource("convention");

            string requestUri = string.Format("{0}/convention/EfCustomers?{1}", BaseAddress, orderby);

            HttpResponseMessage response = await Client.GetAsync(requestUri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject content = await response.Content.ReadAsObject<JObject>();

            Assert.Equal(5, content["value"].Count());

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append(content["value"][i]["Id"]).Append(" > ");
            }
            sb.Remove(sb.Length - 3, 3); // remove the last " > "

            Assert.Equal(expect, sb.ToString());
        }

        private async Task<HttpResponseMessage> ResetDatasource(string mode)
        {
            var requestUriForPost = this.BaseAddress + "/" + mode + "/ResetDataSource";
            var responseForPost = await this.Client.PostAsync(requestUriForPost, new StringContent(""));
            Assert.True(responseForPost.IsSuccessStatusCode);
            return responseForPost;
        }
    }
}
