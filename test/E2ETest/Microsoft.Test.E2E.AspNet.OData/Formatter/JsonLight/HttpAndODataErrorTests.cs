//-----------------------------------------------------------------------------
// <copyright file="HttpAndODataErrorTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class HttpError_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class HttpAndODataErrorAlwaysIncludeDetailsTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.EnableQuerySupport(new QueryableAttribute() { ResultLimit = 100 });
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<HttpError_Todo>("HttpError_Todo");
            return mb.GetEdmModel();
        }

        [Theory]
        [InlineData("/api/HttpError_Todo/ThrowExceptionInAction", HttpStatusCode.InternalServerError, "ThrowExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ThrowHttpResponseExceptionInAction", HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ResponseErrorResponseInAction", HttpStatusCode.NotFound, "ResponseErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/ResponseHttpErrorResponseInAction", HttpStatusCode.NotFound, "ResponseHttpErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/QueryableThrowException", HttpStatusCode.InternalServerError, "Ensure the type of the returned content is IEnumerable, IQueryable, or a generic form of either interface")]
        [InlineData("/api/HttpError_Todo/NotSupportGetException", HttpStatusCode.MethodNotAllowed, "The requested resource does not support http method 'GET'.")]
        [InlineData("/api/HttpError_Todo/ActionNotFound", HttpStatusCode.NotFound, "No HTTP resource was found that matches the request URI")]
        public void TestHttpErrorInActionFull(string url, HttpStatusCode code, string message)
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            Assert.Equal(code, response.StatusCode);
            Assert.Contains(message, response.Content.ReadAsStringAsync().Result);
        }

        private void SetupJsonLight()
        {
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
        }

        [Fact]
        public void TestODataErrorInActionFull()
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataError/ReturnODataErrorResponseInAction").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("ReturnODataErrorResponseInActionMessage", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionCode", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionException", response.Content.ReadAsStringAsync().Result);
        }
    }

    public class HttpAndODataErrorNeverIncludeDetailsTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.EnableQuerySupport(new QueryableAttribute() { ResultLimit = 100 });
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<HttpError_Todo>("HttpError_Todo");
            return mb.GetEdmModel();
        }

        private void SetupJsonLight()
        {
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=fullmetadata"));
        }

        [Theory]
        [InlineData("/api/HttpError_Todo/ThrowExceptionInAction", HttpStatusCode.InternalServerError, "ThrowExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ThrowHttpResponseExceptionInAction", HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ResponseErrorResponseInAction", HttpStatusCode.NotFound, "ResponseErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/QueryableThrowException", HttpStatusCode.InternalServerError, "Ensure the type of the returned content is IEnumerable, IQueryable, or a generic form of either interface")]
        public void TestHttpErrorInActionFull(string url, HttpStatusCode code, string message)
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            Assert.Equal(code, response.StatusCode);
            Assert.DoesNotContain(message, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void TestODataErrorInActionFull()
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataError/ReturnODataErrorResponseInAction").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("ReturnODataErrorResponseInActionMessage", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionCode", response.Content.ReadAsStringAsync().Result);
            Assert.DoesNotContain("ReturnODataErrorResponseInActionException", response.Content.ReadAsStringAsync().Result);
        }
    }

    public class HttpAndODataErrorAlwaysIncludeDetailsTestsMinimal : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.EnableQuerySupport(new QueryableAttribute() { ResultLimit = 100 });
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<HttpError_Todo>("HttpError_Todo");
            return mb.GetEdmModel();
        }

        [Theory]
        [InlineData("/api/HttpError_Todo/ThrowExceptionInAction", HttpStatusCode.InternalServerError, "ThrowExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ThrowHttpResponseExceptionInAction", HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ResponseErrorResponseInAction", HttpStatusCode.NotFound, "ResponseErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/ResponseHttpErrorResponseInAction", HttpStatusCode.NotFound, "ResponseHttpErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/QueryableThrowException", HttpStatusCode.InternalServerError, "Ensure the type of the returned content is IEnumerable, IQueryable, or a generic form of either interface")]
        [InlineData("/api/HttpError_Todo/NotSupportGetException", HttpStatusCode.MethodNotAllowed, "The requested resource does not support http method 'GET'.")]
        [InlineData("/api/HttpError_Todo/ActionNotFound", HttpStatusCode.NotFound, "No HTTP resource was found that matches the request URI")]
        public void TestHttpErrorInActionMinimal(string url, HttpStatusCode code, string message)
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            Assert.Equal(code, response.StatusCode);
            Assert.Contains(message, response.Content.ReadAsStringAsync().Result);
        }

        private void SetupJsonLight()
        {
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
        }

        [Fact]
        public void TestODataErrorInActionMinimal()
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataError/ReturnODataErrorResponseInAction").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("ReturnODataErrorResponseInActionMessage", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionCode", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionException", response.Content.ReadAsStringAsync().Result);
        }
    }

    public class HttpAndODataErrorNeverIncludeDetailsTestsMinimal : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
            configuration.EnableQuerySupport(new QueryableAttribute() { ResultLimit = 100 });
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<HttpError_Todo>("HttpError_Todo");
            return mb.GetEdmModel();
        }

        private void SetupJsonLight()
        {
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=minimalmetadata"));
        }

        [Theory]
        [InlineData("/api/HttpError_Todo/ThrowExceptionInAction", HttpStatusCode.InternalServerError, "ThrowExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ThrowHttpResponseExceptionInAction", HttpStatusCode.NotFound, "ThrowHttpResponseExceptionInAction")]
        [InlineData("/api/HttpError_Todo/ResponseErrorResponseInAction", HttpStatusCode.NotFound, "ResponseErrorResponseInAction")]
        [InlineData("/api/HttpError_Todo/QueryableThrowException", HttpStatusCode.InternalServerError, "Ensure the type of the returned content is IEnumerable, IQueryable, or a generic form of either interface")]
        public void TestHttpErrorInActionMinimal(string url, HttpStatusCode code, string message)
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            Assert.Equal(code, response.StatusCode);
            Assert.DoesNotContain(message, response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void TestODataErrorInActionMinimal()
        {
            SetupJsonLight();
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataError/ReturnODataErrorResponseInAction").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("ReturnODataErrorResponseInActionMessage", response.Content.ReadAsStringAsync().Result);
            Assert.Contains("ReturnODataErrorResponseInActionCode", response.Content.ReadAsStringAsync().Result);
            Assert.DoesNotContain("ReturnODataErrorResponseInActionException", response.Content.ReadAsStringAsync().Result);
        }
    }
}
