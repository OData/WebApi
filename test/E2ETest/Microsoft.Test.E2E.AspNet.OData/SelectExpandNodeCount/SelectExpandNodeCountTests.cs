//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

#if NETCORE
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.SelectExpandNodeCount
{
    public class SelectExpandNodeCountItem
    {
        public int Id { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string P3 { get; set; }
        public string P4 { get; set; }
        public string P5 { get; set; }
        public SelectExpandNodeCountItem Related { get; set; }
    }

    internal static class SelectExpandNodeCountDataSource
    {
        public static readonly IList<SelectExpandNodeCountItem> Items = CreateItems();

        private static IList<SelectExpandNodeCountItem> CreateItems()
        {
            var third = new SelectExpandNodeCountItem
            {
                Id = 3,
                P1 = "a3",
                P2 = "b3",
                P3 = "c3",
                P4 = "d3",
                P5 = "e3"
            };
            var second = new SelectExpandNodeCountItem
            {
                Id = 2,
                P1 = "a2",
                P2 = "b2",
                P3 = "c2",
                P4 = "d2",
                P5 = "e2",
                Related = third
            };
            var first = new SelectExpandNodeCountItem
            {
                Id = 1,
                P1 = "a1",
                P2 = "b1",
                P3 = "c1",
                P4 = "d1",
                P5 = "e1",
                Related = second
            };

            return new List<SelectExpandNodeCountItem> { first, second, third };
        }
    }

    public class LimitedProjectionItemsController : TestODataController
    {
        [EnableQuery(MaxSelectExpandNodeCount = 5)]
        public IQueryable<SelectExpandNodeCountItem> Get()
        {
            return SelectExpandNodeCountDataSource.Items.AsQueryable();
        }
    }

    public class DefaultProjectionItemsController : TestODataController
    {
        [EnableQuery(MaxExpansionDepth = 0)]
        public IQueryable<SelectExpandNodeCountItem> Get()
        {
            return SelectExpandNodeCountDataSource.Items.AsQueryable();
        }
    }

    public class LevelsProjectionItemsController : TestODataController
    {
        [EnableQuery(MaxSelectExpandNodeCount = 12, MaxExpansionDepth = 4)]
        public IQueryable<SelectExpandNodeCountItem> Get()
        {
            return SelectExpandNodeCountDataSource.Items.AsQueryable();
        }
    }

    public class BatchProjectionItemsController : TestODataController
    {
        [EnableQuery(MaxSelectExpandNodeCount = 20, MaxExpansionDepth = 0)]
        public IQueryable<SelectExpandNodeCountItem> Get()
        {
            return SelectExpandNodeCountDataSource.Items.AsQueryable();
        }
    }

    public class UnlimitedDepthLevelsProjectionItemsController : TestODataController
    {
        [EnableQuery(MaxSelectExpandNodeCount = 5, MaxExpansionDepth = 0)]
        public IQueryable<SelectExpandNodeCountItem> Get()
        {
            return SelectExpandNodeCountDataSource.Items.AsQueryable();
        }
    }

    public class SelectExpandNodeCountTests : WebHostTestBase
    {
        private const string ErrorMessage =
            "The number of $select and $expand items in the request exceeds the maximum number allowed of '{0}'.";

        public SelectExpandNodeCountTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            ConfigureRoute(configuration, includeBatch: true);
        }

        [Theory]
        [InlineData("$select=Id,P1,P2,P3,P4,P5")]
        [InlineData("$select=Id,P1,P2,P3,P4&$expand=Related")]
        [InlineData("$expand=Related($select=Id,P1,P2,P3,P4,P5)")]
        public async Task LimitedEndpoint_RequestExceedingLimit_ReturnsBadRequest(string query)
        {
            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/LimitedProjectionItems?" + query.Replace(" ", "%20"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(string.Format(ErrorMessage, 5), await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("")]
        [InlineData("$select=Id")]
        [InlineData("$select=Id,P1,P2,P3,P4")]
        [InlineData("$select=*")]
        [InlineData("$expand=Related")]
        [InlineData("$select=Id,P1,P2,P3&$expand=Related")]
        public async Task LimitedEndpoint_RequestWithinLimit_ReturnsOk(string query)
        {
            string uri = BaseAddress + "/odata/LimitedProjectionItems";
            if (!string.IsNullOrEmpty(query))
            {
                uri += "?" + query.Replace(" ", "%20");
            }

            Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync(uri)).StatusCode);
        }

        [Theory]
        [InlineData("$select=Id,P1,P2,P3,P4,P5")]
        [InlineData("$expand=Related($select=Id,P1,P2,P3,P4,P5)")]
        public async Task DefaultEndpoint_SameRequests_ReturnOk(string query)
        {
            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/DefaultProjectionItems?" + query.Replace(" ", "%20"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("$expand=Related($select=P1,P2,P3;$levels=4)")]
        [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=3)")]
        [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=4)")]
        public async Task LevelsEndpoint_RequestExceedingWeightedLimit_ReturnsBadRequest(string query)
        {
            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/LevelsProjectionItems?" + query);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(string.Format(ErrorMessage, 12), await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("$expand=Related($select=P1;$levels=max)", HttpStatusCode.OK)]
        [InlineData("$expand=Related($select=P1,P2;$levels=max)", HttpStatusCode.BadRequest)]
        public async Task UnlimitedDepthEndpoint_LevelsMaxUsesDefaultLiteralExpansionDepth(
            string query,
            HttpStatusCode expectedStatus)
        {
            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/UnlimitedDepthLevelsProjectionItems?" + query);

            Assert.Equal(expectedStatus, response.StatusCode);
            if (expectedStatus == HttpStatusCode.BadRequest)
            {
                Assert.Contains(string.Format(ErrorMessage, 5), await response.Content.ReadAsStringAsync());
            }
        }

        [Theory]
        [InlineData("$expand=Related($select=P1,P2;$levels=4)")]
        [InlineData("$expand=Related($select=P1,P2,P3,P4,P5;$levels=2)")]
        [InlineData("$expand=Related($select=P1;$levels=4)")]
        [InlineData("$expand=Related($select=P1,P2,P3;$levels=1)")]
        public async Task LevelsEndpoint_RequestWithinWeightedLimit_ReturnsOk(string query)
        {
            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/LevelsProjectionItems?" + query);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LevelsEndpoint_LevelsAloneCausesRejection()
        {
            string baseUri = BaseAddress + "/odata/LevelsProjectionItems?$expand=Related($select=P1,P2,P3";

            Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync(baseUri + ")")).StatusCode);

            HttpResponseMessage response = await Client.GetAsync(baseUri + ";$levels=4)");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(string.Format(ErrorMessage, 12), await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData(5, "400 Bad Request")]
        [InlineData(4, "200 OK")]
        public async Task BatchRequest_EnforcesLimitAcrossNestedProjection(int depth, string expectedStatus)
        {
            HttpResponseMessage response = await Client.SendAsync(CreateBatchGetRequest(depth));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedStatus, body);
            if (depth == 5)
            {
                Assert.Contains(string.Format(ErrorMessage, 20), body);
            }
        }

        internal static void ConfigureRoute(WebRouteConfiguration configuration, bool includeBatch)
        {
            configuration.AddControllers(
                typeof(LimitedProjectionItemsController),
                typeof(DefaultProjectionItemsController),
                typeof(LevelsProjectionItemsController),
                typeof(BatchProjectionItemsController),
                typeof(UnlimitedDepthLevelsProjectionItemsController));
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();

            IEdmModel model = GetEdmModel(configuration);
            if (includeBatch)
            {
                configuration.MapODataServiceRoute(
                    "selectExpandNodeCount",
                    "odata",
                    model,
                    new Microsoft.AspNet.OData.Routing.DefaultODataPathHandler(),
                    Microsoft.AspNet.OData.Routing.Conventions.ODataRoutingConventions.CreateDefault(),
                    configuration.CreateDefaultODataBatchHandler());
            }
            else
            {
                configuration.MapODataServiceRoute("selectExpandNodeCount", "odata", model);
            }
        }

        internal static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<SelectExpandNodeCountItem>("LimitedProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("DefaultProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("LevelsProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("BatchProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("UnlimitedDepthLevelsProjectionItems");
            return builder.GetEdmModel();
        }

        private HttpRequestMessage CreateBatchGetRequest(int depth)
        {
            string query = "$expand=" + BuildNestedExpand(depth);
            string boundary = "batch_" + Guid.NewGuid();
            string body =
                "--" + boundary + "\r\n" +
                "Content-Type: application/http\r\n" +
                "Content-Transfer-Encoding: binary\r\n\r\n" +
                "GET BatchProjectionItems?" + query.Replace(" ", "%20") + " HTTP/1.1\r\n" +
                "Accept: application/json\r\n\r\n" +
                "--" + boundary + "--\r\n";

            var request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/odata/$batch");
            request.Content = new StringContent(body);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/mixed; boundary=" + boundary);
            return request;
        }

        private static string BuildNestedExpand(int depth)
        {
            return "Related(" +
                string.Concat(Enumerable.Repeat("$select=P1,P2,P3,P4;$expand=Related(", depth - 1)) +
                "$select=P1,P2,P3,P4" +
                new string(')', depth);
        }
    }

    public class SelectExpandNodeCountGlobalTests : WebHostTestBase
    {
        public SelectExpandNodeCountGlobalTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MaxSelectExpandNodeCount(5);
            SelectExpandNodeCountTests.ConfigureRoute(configuration, includeBatch: false);
        }

        [Fact]
        public async Task DefaultEndpoint_UsesGlobalLimit()
        {
            HttpResponseMessage exceeding = await Client.GetAsync(
                BaseAddress + "/odata/DefaultProjectionItems?$select=Id,P1,P2,P3,P4,P5");
            Assert.Equal(HttpStatusCode.BadRequest, exceeding.StatusCode);
            Assert.Contains("maximum number allowed of '5'", await exceeding.Content.ReadAsStringAsync());

            HttpResponseMessage within = await Client.GetAsync(
                BaseAddress + "/odata/DefaultProjectionItems?$select=Id,P1,P2,P3,P4");
            Assert.Equal(HttpStatusCode.OK, within.StatusCode);
        }
    }

#if NETCORE
    public class SelectExpandNodeCountLoggingTests : WebHostTestBase
    {
        private static readonly CapturingLoggerProvider LoggerProvider = new CapturingLoggerProvider();

        public SelectExpandNodeCountLoggingTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            SelectExpandNodeCountTests.ConfigureRoute(configuration, includeBatch: false);
            configuration.GetDefaultODataOptions().EnableQueryValidationErrorLogging = true;
            configuration.ServiceProvider.GetRequiredService<ILoggerFactory>().AddProvider(LoggerProvider);
        }

        [Fact]
        public async Task RejectedRequest_WritesQueryValidationDiagnostic()
        {
            LoggerProvider.Clear();

            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/LimitedProjectionItems?$select=Id,P1,P2,P3,P4,P5");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            CapturedLogEntry entry = Assert.Single(LoggerProvider.Entries.Where(
                e => e.Category == typeof(EnableQueryAttribute).FullName));
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal("$select=Id,P1,P2,P3,P4,P5", entry.GetFieldValue("QueryOptions"));
            Assert.Contains("maximum number allowed of '5'", entry.Exception.Message);
        }

        [Fact]
        public async Task AcceptedRequest_WritesNoQueryValidationDiagnostic()
        {
            LoggerProvider.Clear();

            HttpResponseMessage response = await Client.GetAsync(
                BaseAddress + "/odata/LimitedProjectionItems?$select=Id,P1,P2,P3,P4");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(LoggerProvider.Entries.Where(
                e => e.Category == typeof(EnableQueryAttribute).FullName));
        }
    }

    internal sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedLogEntry> _entries = new ConcurrentQueue<CapturedLogEntry>();

        public IReadOnlyList<CapturedLogEntry> Entries => _entries.ToArray();

        public void Clear()
        {
            while (_entries.TryDequeue(out _))
            {
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, _entries);
        }

        public void Dispose()
        {
        }

        private sealed class CapturingLogger : ILogger
        {
            private readonly string _category;
            private readonly ConcurrentQueue<CapturedLogEntry> _entries;

            public CapturingLogger(string category, ConcurrentQueue<CapturedLogEntry> entries)
            {
                _category = category;
                _entries = entries;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var fields = new Dictionary<string, object>();
                if (state is IReadOnlyList<KeyValuePair<string, object>> values)
                {
                    foreach (KeyValuePair<string, object> value in values)
                    {
                        fields[value.Key] = value.Value;
                    }
                }

                _entries.Enqueue(new CapturedLogEntry
                {
                    Category = _category,
                    Level = logLevel,
                    Exception = exception,
                    Fields = fields
                });
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }

    internal sealed class CapturedLogEntry
    {
        public string Category { get; set; }
        public LogLevel Level { get; set; }
        public Exception Exception { get; set; }
        public IReadOnlyDictionary<string, object> Fields { get; set; }

        public string GetFieldValue(string name)
        {
            return Fields.TryGetValue(name, out object value) ? value?.ToString() : null;
        }
    }
#endif
}
