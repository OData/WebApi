//-----------------------------------------------------------------------------
// <copyright file="FilterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Products;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class FilterTests : WebHostTestBase
    {
        #region Test Data
        public static TheoryDataSet<string, IEnumerable<Product>> SpecialCharacterData
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();

                foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
                {
                    var expected = products.Where(p => (p.Name == null ? false : p.Name.Contains(c))).ToList();
                    data.Add(Uri.EscapeDataString(string.Format("contains(Name, '{0}')", c)), expected);
                }

                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Product>> OperatorData
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();
                string name = products.First(p => p.Name != null && !p.Name.Contains("&") && !p.Name.Contains("+") && !p.Name.Contains("#") && !p.Name.Contains("%")).Name;
                data.Add("1 eq 1", products);
                data.Add(string.Format("Name eq '{0}'", Encoding(name)), products.Where(p => p.Name == name));
                data.Add(string.Format("Name ne '{0}'", Encoding(name)), products.Where(p => p.Name != name));
                data.Add(string.Format("Name gt '{0}'", Encoding(name)), products.Where(p => string.Compare(p.Name, name) > 0));
                data.Add(string.Format("Name lt '{0}'", Encoding(name)), products.Where(p => string.Compare(p.Name, name) < 0));
                data.Add("ID gt 1", products.Where(p => p.ID > 1));
                data.Add("ID ge 1", products.Where(p => p.ID >= 1));
                data.Add("ID lt 20", products.Where(p => p.ID < 20));
                data.Add("ID le 20", products.Where(p => p.ID <= 20));
                data.Add("ID le 20 and ID gt 1", products.Where(p => p.ID <= 20 && p.ID > 1));
                data.Add("ID le 20 or ID gt 1", products.Where(p => p.ID <= 20 || p.ID > 1));
                data.Add("not (ID le 20)", products.Where(p => !(p.ID <= 20)));
                data.Add("Price add 1000 gt 2000", products.Where(p => (p.Price + 1000) > 2000));
                data.Add("Price sub 1000 gt 2000", products.Where(p => (p.Price - 1000) > 2000));
                data.Add("Price mul 1 gt 2000", products.Where(p => (p.Price * 1) > 2000));
                data.Add("Price div 10 gt 2000", products.Where(p => (p.Price / 10) > 2000));
                data.Add("Price mod 2 gt 1", products.Where(p => (p.Price % 2) > 1));
                data.Add("DiscontinuedDate eq null", products.Where(p => p.DiscontinuedDate == null));
                data.Add("DiscontinuedDate eq 2012-12-31T12:00:00Z", products.Where(p => p.DiscontinuedDate == new DateTimeOffset(2012, 12, 31, 12, 0, 0, TimeSpan.Zero)));
                //data.Add("DiscontinuedDate eq datetime'2012-12-31T12:00'", products.Where(p => p.DiscontinuedDate == new DateTime(2012, 12, 31, 12, 0, 0))); // 504285
                data.Add("DateTimeOffset eq 2011-06-01T14:03:00Z",
                    products.Where(p => p.DateTimeOffset.HasValue ? p.DateTimeOffset == new DateTimeOffset(2011, 6, 1, 14, 3, 0, TimeSpan.Zero) : false));
                data.Add("Supplier/Address eq null",
                    products.Where(p => p.Supplier == null ? true : p.Supplier.Address == null));
                data.Add("Taxable", products.Where(p => p.Taxable ?? false));
                return data;
            }
        }

        //public static TheoryDataSet<string, IEnumerable<Product>> NavigationPropertyData
        //{
        //    get
        //    {
        //        var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
        //        var data = new TheoryDataSet<string, IEnumerable<Product>>();
        //        string name = products.First().Supplier.Name;
        //        data.Add("contains('a', Name) eq true", products.Where(p => p.Name == null ? false : p.Name.Contains("a")));
        //    }
        //}

        public static TheoryDataSet<string, IEnumerable<Product>> StringFunctionData
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();
                string name = products.First().Name;
                data.Add("contains(Name, 'a') eq true", products.Where(p => p.Name == null ? false : p.Name.Contains("a")));
                data.Add("endswith(Name, 'a') eq true", products.Where(p => p.Name == null ? false : p.Name.EndsWith("a")));
                data.Add("startswith(Name, 'a') eq true", products.Where(p => p.Name == null ? false : p.Name.StartsWith("a")));
                data.Add("length(Name) lt 10", products.Where(p => p.Name == null ? false : p.Name.Length < 10));
                data.Add("indexof(Name, 'a') gt 0", products.Where(p => p.Name == null ? false : p.Name.IndexOf("a") > 0));
                data.Add("substring(Name, 8) eq 'abc'", products.Where(p => (p.Name == null || p.Name.Length < 8) ? false : p.Name.Substring(8) == "abc"));
                data.Add("substring(Name, 8, 1) eq 'a'", products.Where(p => (p.Name == null || p.Name.Length < 9) ? false : p.Name.Substring(8, 1) == "a"));
                data.Add("tolower(Name) eq 'a'", products.Where(p => p.Name == null ? false : p.Name.ToLower() == "a"));
                data.Add("toupper(Name) eq 'A'", products.Where(p => p.Name == null ? false : p.Name.ToUpper() == "A"));
                data.Add("trim(Name) eq Name", products.Where(p => p.Name == null ? true : p.Name.Trim() == p.Name));
                data.Add("concat(Name, 'a') eq 'aa'", products.Where(p => p.Name == null ? false : string.Concat(p.Name, "a") == "aa"));
                data.Add("day(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Day > 1));
                data.Add("hour(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Hour > 1));
                data.Add("minute(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Minute > 1));
                data.Add("month(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Month > 1));
                data.Add("year(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Year > 1));
                data.Add("second(ReleaseDate) gt 1", products.Where(p => p.ReleaseDate.HasValue && p.ReleaseDate.Value.Second > 1));
                data.Add("round(Price) gt 0", products.Where(p => Math.Round(p.Price.Value) > 0));
                data.Add("floor(Price) gt 0", products.Where(p => Math.Floor(p.Price.Value) > 0));
                data.Add("ceiling(Price) gt 0", products.Where(p => Math.Ceiling(p.Price.Value) > 0));
                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Product>> DateAndTimeOfDayData
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();

                // Edm.Date
                data.Add("year(Date) lt 1001", products.Where(p => p.Date.Year < 1001));
                data.Add("month(Date) lt 2", products.Where(p => p.Date.Month < 2));
                data.Add("day(Date) lt 2", products.Where(p => p.Date.Day < 2));
                data.Add("year(NullableDate) lt 1001", products.Where(p => p.NullableDate.HasValue && p.NullableDate.Value.Year < 1001));
                data.Add("month(NullableDate) lt 2", products.Where(p => p.NullableDate.HasValue && p.NullableDate.Value.Month < 2));
                data.Add("day(NullableDate) lt 2", products.Where(p => p.NullableDate.HasValue && p.NullableDate.Value.Day < 2));

                // Edm.TimeOfDay
                data.Add("hour(TimeOfDay) lt 2", products.Where(p => p.TimeOfDay.Hours < 2));
                data.Add("minute(TimeOfDay) lt 2", products.Where(p => p.TimeOfDay.Minutes < 2));
                data.Add("second(TimeOfDay) lt 2", products.Where(p => p.TimeOfDay.Seconds < 2));
                data.Add("hour(NullableTimeOfDay) lt 2", products.Where(p => p.NullableTimeOfDay.HasValue && p.NullableTimeOfDay.Value.Hours < 2));
                data.Add("minute(NullableTimeOfDay) lt 2", products.Where(p => p.NullableTimeOfDay.HasValue && p.NullableTimeOfDay.Value.Minutes < 2));
                data.Add("second(NullableTimeOfDay) lt 2", products.Where(p => p.NullableTimeOfDay.HasValue && p.NullableTimeOfDay.Value.Seconds < 2));

                // fractionalseconds
                data.Add("fractionalseconds(ReleaseDate) ge 0.2", products.Where(p => p.ReleaseDate.HasValue && ((decimal)p.ReleaseDate.Value.Millisecond) / 1000 >= 0.2m));
                data.Add("fractionalseconds(TimeOfDay) ge 0.2", products.Where(p => ((decimal)p.TimeOfDay.Milliseconds) / 1000 >= 0.2m));
                data.Add("fractionalseconds(NullableTimeOfDay) ge 0.2", products.Where(p => p.NullableTimeOfDay.HasValue && ((decimal)p.NullableTimeOfDay.Value.Milliseconds) / 1000 >= 0.2m));

                // date()
                data.Add("date(PublishDate) lt 2015-02-26", products.Where(p => new Date(p.PublishDate.Year, p.PublishDate.Month, p.PublishDate.Day) < new Date(2015, 2, 26)));
                data.Add("date(ReleaseDate) lt 2015-02-26", products.Where(p => p.ReleaseDate.HasValue && new Date(p.ReleaseDate.Value.Year, p.ReleaseDate.Value.Month, p.ReleaseDate.Value.Day) < new Date(2015, 2, 26)));

                // time()
                data.Add("time(PublishDate) lt 01:02:03.0040000", products.Where(p => new TimeOfDay(p.PublishDate.Hour, p.PublishDate.Minute, p.PublishDate.Second, p.PublishDate.Millisecond) < new TimeOfDay(1, 2, 3, 4)));
                data.Add("time(ReleaseDate) lt 01:02:03.0040000", products.Where(p => p.ReleaseDate.HasValue && new TimeOfDay(p.ReleaseDate.Value.Hour, p.ReleaseDate.Value.Minute, p.ReleaseDate.Value.Second, p.ReleaseDate.Value.Second) < new TimeOfDay(1, 2, 3, 4)));

                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Movie>> AnyAllData
        {
            get
            {
                var movies = ModelHelper.CreateMovieData().OrderBy(m => m.MovieId);
                var data = new TheoryDataSet<string, IEnumerable<Movie>>();
                //data.Add("Director eq Director", movies.Where(p => p.Director == p.Director)); // 476567
                data.Add("Actors/any(actor: actor/Name eq 'Kevin')", movies.Where(p => p.Actors.Any(a => a.Name == "Kevin")));
                data.Add("Actors/any(actor: actor/Partner/Name eq 'Rose')", movies.Where(p => p.Actors.Any(a => a.Partner != null && a.Partner.Name == "Rose")));
                data.Add("Actors/all(actor: actor/Age gt 20)", movies.Where(p => p.Actors.All(a => a.Age > 20)));
                data.Add("Actors/all(actor: actor/Age gt 30)", movies.Where(p => p.Actors.All(a => a.Age > 30)));
                data.Add("(Actors/all(actor: actor/Age gt 20) and Actors/all(actor: actor/Age le 30))", movies.Where(p => p.Actors.All(a => a.Age > 20 && a.Age <= 30)));
                data.Add("Tags/any(tag: tag eq 'Quirky')", movies.Where(p => p.Tags != null && p.Tags.Any(t => t == "Quirky")));
                //data.Add("Actors/any(actor: actor eq Director)", movies.Where(p => p.Actors.Any(a => a == p.Director))); // 476567
                data.Add("Actors/any(actor: actor/PersonId eq Director/PersonId)", movies.Where(p => p.Actors.Any(a => a.PersonId == p.Director.PersonId)));
                data.Add("Actors/any(actor: actor/Tags/any(tag: tag eq 'Favorite'))", movies.Where(p => p.Actors.Any(a => a.Tags != null && a.Tags.Any(t => t == "Favorite"))));
                data.Add("Actors/any(actor: actor/Movies/any(movie: movie/Actors/any(actor1: actor1/Name eq 'Kevin')))",
                    movies.Where(p => p.Actors.Any(a => a.Movies == null ? false : a.Movies.Any(m => m.Actors == null ? false : m.Actors.Any(a1 => a1.Name == "Kevin")))));

                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Movie>> InOperatorData
        {
            get
            {
                var movies = ModelHelper.CreateMovieData();
                var data = new TheoryDataSet<string, IEnumerable<Movie>>();
                data.Add("100 in Sales", movies.Where(p => p.Sales.Contains(100)));
                data.Add("'Quirky' in Tags", movies.Where(p => p.Tags.Contains("Quirky")));
                data.Add("Director in Actors", movies.Where(p => p.Actors.Contains(p.Director)));
                data.Add("Producer in Actors", movies.Where(p => p.Actors.Contains(p.Producer)));
                data.Add("MovieId in (1, 2, 3)", movies.Where(p => (new int[] { 1, 2, 3 }).Contains(p.MovieId)));
                data.Add("Title in ('movie1','movie3')", movies.Where(p => (new string[] { "movie1", "movie3" }).Contains(p.Title)));
                data.Add("MainGenre in Genres", movies.Where(p => p.Genres.Contains(p.MainGenre)));
                data.Add("Director/Partner in Actors", movies.Where(p => p.Actors.Contains(p.Director.Partner)));
                data.Add("Director/LastVisited in Producer/FavoriteTheaters", movies.Where(
                    p => p.Producer.FavoriteTheaters.Contains(p.Director.LastVisited)));

                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Product>> MixQueries
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();
                data.Add("contains(Name, 'd') eq true and ReleaseDate gt '2001-01-01' and ID gt 0 and ID lt 100",
                    products.Where(p => (p.Name == null ? false : p.Name.Contains("d")) && p.ReleaseDate > new DateTime(2000, 1, 1) && p.ID > 0 && p.ID < 100));
                return data;
            }
        }

        public static TheoryDataSet<string, IEnumerable<Product>> AdHocTests
        {
            get
            {
                var products = ModelHelper.CreateRandomProducts().OrderBy(p => p.ID);
                var data = new TheoryDataSet<string, IEnumerable<Product>>();
                //data.Add("DiscontinuedDate eq datetime'2012-12-31T12:00'", products.Where(p => p.DiscontinuedDate == new DateTime(2012, 12, 31, 12, 0, 0))); // 504285
                return data;
            }
        }
        #endregion

        public FilterTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        private static string Encoding(string name)
        {
            return name.Replace("'", "''").Replace("&", "%26").Replace("/", "%2F").Replace("?", "%3F");
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public async Task TestFilters()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Product> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = SpecialCharacterData
                .Union(OperatorData)
                .Union(StringFunctionData)
                .Union(DateAndTimeOfDayData);
            //.Union(MixQueries) 1559
            //.Union*AdHocTests);

            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Product> expected = (IEnumerable<Product>)testCase[1];

                var requestUri = this.BaseAddress + "/api/FilterTests/GetProducts?$filter=" + filter;

                var response = await this.Client.GetAsync(requestUri);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    /* 
                     * This if statement is added due to that the test data is generated randomly, and sometimes the test case fails on CI,
                     * but we have no way to investigate as both the request and response are not logged. 
                     */

                    // C:\Users\{user}\AppData\Local\Temp\
                    var now = DateTimeOffset.Now;
                    var path = System.IO.Path.GetTempPath() + "FilterTests.TestFilters.Error." + now.ToString("yyyy-MM-dd_HH-mm-ss_fffffff.") + Guid.NewGuid().ToString() + ".log";
                    var traceListener = new TextWriterTraceListener(path, "FilterTests.TestFilters");
                    Trace.Listeners.Add(traceListener);

                    Trace.TraceInformation("Request: {0}", requestUri);
                    Trace.TraceError("StatusCode: {0}", response.StatusCode);
                    Trace.TraceError(await response.Content.ReadAsStringAsync());

                    Trace.Flush();
                    Trace.Listeners.Remove(traceListener);
                    traceListener.Close();
                    Assert.True(false);
                }
                var result = await response.Content.ReadAsObject<IEnumerable<Product>>();

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).ID, result.ElementAt(i).ID);
                    Assert.Equal(expected.ElementAt(i).Name, result.ElementAt(i).Name);
                    Assert.Equal(expected.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
        }

        [Fact]
        public async Task TestFiltersWithXmlSerializer()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Product> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            // Some of these test cases may fail rarely because of the random data - the seed for randomization changes every hour though. 
            var testCases = SpecialCharacterData
                .Union(OperatorData)
                .Union(StringFunctionData)
                .Union(DateAndTimeOfDayData);
            //.Union(MixQueries) 1559
            //.Union*AdHocTests); 396

            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Product> expected = (IEnumerable<Product>)testCase[1];

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/api/FilterTests/GetProducts?$filter=" + filter);
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));
                var response = await this.Client.SendAsync(request);
                var result = await response.Content.ReadAsObject<IEnumerable<Product>>();

                if (expected.Count() == 5 && result.Count() ==8)
                {
                    Assert.True(true);
                }

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).ID, result.ElementAt(i).ID);
                    Assert.Equal(expected.ElementAt(i).Name, result.ElementAt(i).Name);
                    Assert.Equal(expected.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
        }

        [Fact]
        public async Task TestFiltersOnHttpResponse()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Product> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = SpecialCharacterData
                .Union(OperatorData)
                .Union(StringFunctionData)
                .Union(DateAndTimeOfDayData);
            //.Union(MixQueries) 1559
            //.Union*AdHocTests); 396

            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Product> expected = (IEnumerable<Product>)testCase[1];

                var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProductsHttpResponse?$filter=" + filter);
                var result = await response.Content.ReadAsObject<IEnumerable<Product>>();

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).ID, result.ElementAt(i).ID);
                    Assert.Equal(expected.ElementAt(i).Name, result.ElementAt(i).Name);
                    Assert.Equal(expected.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
        }

        [Fact]
        public async Task TestFiltersAsync()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Product> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = SpecialCharacterData
                .Union(OperatorData)
                .Union(StringFunctionData)
                .Union(DateAndTimeOfDayData);
            //.Union(MixQueries) 1559
            //.Union*AdHocTests); 396

            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Product> expected = (IEnumerable<Product>)testCase[1];

                var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetAsyncProducts?$filter=" + filter);
                var result = await response.Content.ReadAsObject<IEnumerable<Product>>();

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).ID, result.ElementAt(i).ID);
                    Assert.Equal(expected.ElementAt(i).Name, result.ElementAt(i).Name);
                    Assert.Equal(expected.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
        }

        [Fact]
        public async Task TestFiltersOnAnonymousType()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Product> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = SpecialCharacterData
                .Union(OperatorData)
                .Union(StringFunctionData)
                .Union(DateAndTimeOfDayData);
            //.Union(MixQueries) 1559
            //.Union*AdHocTests); 396

            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Product> expected = (IEnumerable<Product>)testCase[1];

                var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetProductsAsAnonymousType?$filter=" + filter);
                var result = await response.Content.ReadAsObject<IEnumerable<Product>>();

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).ID, result.ElementAt(i).ID);
                    Assert.Equal(expected.ElementAt(i).Name, result.ElementAt(i).Name);
                    Assert.Equal(expected.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
        }

        [Fact]
        public async Task TestAnyAll()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Movie> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = AnyAllData;
            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Movie> expected = (IEnumerable<Movie>)testCase[1];

                var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetMovies?$filter=" + filter);
                var result = await response.Content.ReadAsObject<IEnumerable<Movie>>();

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).MovieId, result.ElementAt(i).MovieId);
                    Assert.Equal(expected.ElementAt(i).Director.PersonId, result.ElementAt(i).Director.PersonId);
                }
            }
        }

        [Fact]
        public async Task TestInOperator()
        {
            // While this seems ideal for a Theory test case, the IEnumerable<Movie> would need to be serialize-able in
            // order to generate an Xunit 2.0 test case.
            var testCases = InOperatorData;
            foreach (var testCase in testCases)
            {
                string filter = (string)testCase[0];
                IEnumerable<Movie> expected = (IEnumerable<Movie>)testCase[1];

                var response = await this.Client.GetAsync(this.BaseAddress + "/api/FilterTests/GetMovies?$filter=" + filter);
                var result = await response.Content.ReadAsObject<IEnumerable<Movie>>();

                var list = result.ToList();
                Assert.NotNull(list);

                Assert.Equal(expected.Count(), result.Count());
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.Equal(expected.ElementAt(i).MovieId, result.ElementAt(i).MovieId);
                    Assert.Equal(expected.ElementAt(i).Director.PersonId, result.ElementAt(i).Director.PersonId);
                }
            }
        }

        //[Theory(Skip="It is not stable, now disable it to prevent it from hiding other test failures.")]
        //[MemberData(nameof(SpecialCharacterData))]
        //[MemberData(nameof(OperatorData))]
        //[MemberData(nameof(StringFunctionData))]
        //[MemberData(nameof(MixQueries))] 1559
        //[MemberData(nameof(AdHocTests))]
        protected void TestFiltersWithMultipleThreads(string filter, IEnumerable<Product> expected)
        {
            Parallel.For(0, 10, i =>
            {
                //TestFilters(filter, expected);
            });
        }
    }
}
