// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Http.Validation;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    /// <summary>
    /// NonODataController is an abstracted ASP.NET [Core] controller.
    /// </summary>
#if NETCORE
    public class TestNonODataController : ControllerBase
#else
    public class TestNonODataController : ApiController
#endif
    {
        [NonAction]
        public new TestNotFoundResult NotFound() { return new TestNotFoundResult(base.NotFound()); }

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }

        [NonAction]
        public TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(value); }
#else
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
#endif

        [NonAction]
        public new TestBadRequestResult BadRequest() { return new TestBadRequestResult(base.BadRequest()); }

        [NonAction]
#if NETCORE
        public TestBadRequestObjectResult BadRequest(string message) { return new TestBadRequestObjectResult(base.BadRequest(message)); }
#else
        public new TestBadRequestObjectResult BadRequest(string message) { return new TestBadRequestObjectResult(base.BadRequest(message)); }

#endif

#if NETCORE
        public new TestBadRequestObjectResult BadRequest(object obj) { return new TestBadRequestObjectResult(base.BadRequest(obj)); }
#else
        public TestBadRequestObjectResult<T> BadRequest<T>(T obj) { return new TestBadRequestObjectResult<T>(base.Content(HttpStatusCode.BadRequest, obj)); }
#endif
    }

    /// <summary>
    /// TestController is a controller designed to be used in UnitTests to abstract the controller
    /// semantics between AspNet and AspNet core. TestController implements (and hides) the convenience
    /// methods for generating responses and surfaces those as a common type, ITestActionResult.
    /// ITestActionResult is derived from the AspNet/AspNetCore and implements the correct ActionResult
    /// interface.
    /// </summary>
    public class TestODataController : ODataController
    {
        [NonAction]
        public new TestNotFoundResult NotFound() { return new TestNotFoundResult(base.NotFound()); }

        [NonAction]
#if NETCORE
        public new TestNotFoundObjectResult NotFound(object value) { return new TestNotFoundObjectResult(base.NotFound(value)); }
#else
        public TestNotFoundObjectResult NotFound(object value) { return new TestNotFoundObjectResult(base.NotFound()); }
#endif

#if NETCORE
        public new TestNotFoundResult NotFound(ODataError error) { return new TestNotFoundResult(base.NotFound(error)); }
#endif

        [NonAction]
        public new TestBadRequestResult BadRequest() { return new TestBadRequestResult(base.BadRequest()); }

        [NonAction]
#if NETCORE
        public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState) { return new TestBadRequestObjectResult(base.BadRequest(modelState)); }
#else
        public new TestBadRequestObjectResult BadRequest(ModelStateDictionary modelState) { return new TestBadRequestObjectResult(base.BadRequest(modelState)); }
#endif

        [NonAction]
#if NETCORE
        public new TestBadRequestResult BadRequest(string message) { return new TestBadRequestResult(base.BadRequest(message)); }
#else
        public new TestBadRequestObjectResult BadRequest(string message) { return new TestBadRequestObjectResult(base.BadRequest(message)); }

#endif

        [NonAction]
#if NETCORE
        public new TestBadRequestResult BadRequest(ODataError error) { return new TestBadRequestResult(base.BadRequest(error)); }
#endif

#if NETCORE
        public new TestBadRequestObjectResult BadRequest(object obj) { return new TestBadRequestObjectResult(base.BadRequest(obj)); }
#else
        public TestBadRequestObjectResult<T> BadRequest<T>(T obj) { return new TestBadRequestObjectResult<T>(base.Content(HttpStatusCode.BadRequest, obj)); }
#endif

#if NETCORE
        [NonAction]
        public new TestUnauthorizedResult Unauthorized() { return new TestUnauthorizedResult(base.Unauthorized()); }
#endif

#if NETCORE
        [NonAction]
        public new TestUnauthorizedResult Unauthorized(string message) { return new TestUnauthorizedResult(base.Unauthorized(message)); }
#endif

#if NETCORE
        [NonAction]
        public new TestUnauthorizedResult Unauthorized(ODataError error) { return new TestUnauthorizedResult(base.Unauthorized(error)); }
#endif

#if NETCOREAPP3_1
        [NonAction]
        public new TestConflictResult Conflict() { return new TestConflictResult(base.Conflict()); }

        [NonAction]
        public new TestConflictResult Conflict(string message) { return new TestConflictResult(base.Conflict(message)); }

        [NonAction]
        public new TestConflictResult Conflict(ODataError error) { return new TestConflictResult(base.Conflict(error)); }

        [NonAction]
        public new TestUnprocessableEntityResult UnprocessableEntity() { return new TestUnprocessableEntityResult(base.UnprocessableEntity()); }

        [NonAction]
        public new TestUnprocessableEntityResult UnprocessableEntity(string message) { return new TestUnprocessableEntityResult(base.UnprocessableEntity(message)); }

        [NonAction]
        public new TestUnprocessableEntityResult UnprocessableEntity(ODataError error) { return new TestUnprocessableEntityResult(base.UnprocessableEntity(error)); }
#endif

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }
#else
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
#endif

        [NonAction]
#if NETCORE
        public TestStatusCodeResult StatusCode(HttpStatusCode statusCode) { return new TestStatusCodeResult(base.StatusCode((int)statusCode)); }
#else
        public new TestStatusCodeResult StatusCode(HttpStatusCode statusCode) { return new TestStatusCodeResult(base.StatusCode(statusCode)); }
#endif

        [NonAction]
#if NETCORE
        public TestStatusCodeObjectResult StatusCode(HttpStatusCode statusCode, object value) { return new TestStatusCodeObjectResult(base.StatusCode((int)statusCode, value)); }
#else
        public TestStatusCodeObjectResult StatusCode(HttpStatusCode statusCode, object value) { return new TestStatusCodeObjectResult(base.Content(statusCode, value)); }
#endif

        [NonAction]
#if NETCORE
        public new TestCreatedODataResult<T> Created<T>(T entity) { return new TestCreatedODataResult<T>(entity); }
#else
        public new TestCreatedODataResult<T> Created<T>(T entity) { return new TestCreatedODataResult<T>(entity, this); }
#endif

        [NonAction]
#if NETCORE
        public new TestCreatedResult Created(string uri, object entity) { return new TestCreatedResult(base.Created(uri, entity)); }
#else
        public new TestCreatedResult<T> Created<T>(string uri, T entity) { return new TestCreatedResult<T>(base.Created<T>(uri, entity)); }
#endif

        [NonAction]
#if NETCORE
        public new TestUpdatedODataResult<T> Updated<T>(T entity) { return new TestUpdatedODataResult<T>(entity); }
#else
        public new TestUpdatedODataResult<T> Updated<T>(T entity) { return new TestUpdatedODataResult<T>(entity, this); }
#endif

        protected string GetServiceRootUri()
        {
#if NETCORE
            string routeName = Request.ODataFeature().RouteName;
            StringBuilder requestLeftPartBuilder = new StringBuilder(Request.Scheme);
            requestLeftPartBuilder.Append("://");
            requestLeftPartBuilder.Append(Request.Host.HasValue ? Request.Host.Value : Request.Host.ToString());
            if (!string.IsNullOrEmpty(routeName))
            {
                requestLeftPartBuilder.Append("/");
                requestLeftPartBuilder.Append(routeName);
            }

            return requestLeftPartBuilder.ToString();
#else
            var routeName = Request.ODataProperties().RouteName;
            ODataRoute odataRoute = Configuration.Routes[routeName] as ODataRoute;
            var prefixName = odataRoute.RoutePrefix;
            var requestUri = Request.RequestUri.ToString();
            var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName) + prefixName.Length);
            return serviceRootUri;
#endif
        }

        protected string GetRoutePrefix()
        {
#if NETCORE
            ODataRoute oDataRoute = Request.HttpContext.GetRouteData().Routers
                .Where(r => r.GetType() == typeof(ODataRoute))
                .SingleOrDefault() as ODataRoute;
#else
            ODataRoute oDataRoute = Request.GetRouteData().Route as ODataRoute;
#endif
            Assert.NotNull(oDataRoute);
            return oDataRoute.RoutePrefix;
        }

        protected T GetRequestValue<T>(Uri value)
        {
            return Request.GetKeyValue<T>(value);
        }

#if NETCORE
        protected bool Validate(object model)
        {
            return TryValidateModel(model);
        }
#endif
    }

    /// <summary>
    /// Wrapper for NotFoundResult
    /// </summary>
#if NETCORE
    public class TestNotFoundResult : TestStatusCodeResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestNotFoundResult : TestActionResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for NotFoundObjectResult
    /// </summary>
#if NETCORE
    public class TestNotFoundObjectResult : TestObjectResult
    {
        public TestNotFoundObjectResult(NotFoundObjectResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestNotFoundObjectResult : TestActionResult
    {
        public TestNotFoundObjectResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for OkResult
    /// </summary>
#if NETCORE
    public class TestOkResult : TestStatusCodeResult
    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestOkResult : TestActionResult

    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for OkObjectResult
    /// </summary>
#if NETCORE
    public class TestOkObjectResult : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
        }
    }

    public class TestOkObjectResult<T> : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
        }

        public TestOkObjectResult(T content, TestODataController controller)
            : base(content)
        {
            // Controller is unused.
            this.StatusCode = 200;
        }
    }
#else
    public class TestOkObjectResult<T> : TestActionResult
    {
        public TestOkObjectResult(OkNegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }

        public TestOkObjectResult(T content, TestODataController controller)
        {
            this.innerResult = new OkNegotiatedContentResult<T>(content, controller);
        }
    }
#endif

    /// <summary>
    /// Wrapper for BadRequestResult
    /// </summary>
#if NETCORE
    public class TestBadRequestResult : TestActionResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    /// <summary>
    /// Wrapper for BadRequestResult
    /// </summary>
    public class TestBadRequestResult : TestActionResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for BadRequestObjectResult
    /// </summary>
#if NETCORE
    public class TestBadRequestObjectResult : TestActionResult
    {
        public TestBadRequestObjectResult(BadRequestObjectResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestBadRequestObjectResult : TestActionResult
    {
        public TestBadRequestObjectResult(InvalidModelStateResult innerResult)
            : base(innerResult)
        {
        }

        public TestBadRequestObjectResult(BadRequestErrorMessageResult innerResult)
            : base(innerResult)
        {
        }
    }

    public class TestBadRequestObjectResult<T> : TestActionResult
    {
        public TestBadRequestObjectResult(NegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for UnauthorizedResult
    /// </summary>
#if NETCORE
    public class TestUnauthorizedResult : TestActionResult
    {
        public TestUnauthorizedResult(UnauthorizedResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for ConflictResult
    /// </summary>
#if NETCOREAPP3_1
    public class TestConflictResult : TestActionResult
    {
        public TestConflictResult(ConflictResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for UnprocessableEntityResult
    /// </summary>
#if NETCOREAPP3_1
    public class TestUnprocessableEntityResult : TestActionResult
    {
        public TestUnprocessableEntityResult(UnprocessableEntityResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for StatusCodeObjectResult
    /// </summary>
#if NETCORE
    public class TestStatusCodeObjectResult : TestObjectResult
    {
        public TestStatusCodeObjectResult(ObjectResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestStatusCodeObjectResult : TestActionResult
    {
        public TestStatusCodeObjectResult(NegotiatedContentResult<object> innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for CreatedODataResult
    /// </summary>
#if NETCORE
    public class TestCreatedODataResult<T> : CreatedODataResult<T>, ITestActionResult
    {
        public TestCreatedODataResult(T entity)
            : base(entity)
        {
        }

        public TestCreatedODataResult(string uri, T entity)
            : base(entity)
        {
        }
    }
#else
    public class TestCreatedODataResult<T> : CreatedODataResult<T>, ITestActionResult
    {
        public TestCreatedODataResult(T entity, ApiController controller)
            : base(entity, controller)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for CreatedResult
    /// </summary>
#if NETCORE
    public class TestCreatedResult: TestActionResult
    {
        public TestCreatedResult(CreatedResult innerResult)
            : base(innerResult)
        {
        }
    }
#else
    public class TestCreatedResult<T> : TestActionResult
    {
        public TestCreatedResult(CreatedNegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Wrapper for UpdatedODataResult
    /// </summary>
#if NETCORE
    public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, ITestActionResult
    {
        public TestUpdatedODataResult(T entity)
            : base(entity)
        {
        }

        public TestUpdatedODataResult(string uri, T entity)
            : base(entity)
        {
        }
    }
#else
    public class TestUpdatedODataResult<T> : UpdatedODataResult<T>, ITestActionResult
    {
        public TestUpdatedODataResult(T entity, ApiController controller)
            : base(entity, controller)
        {
        }
    }
#endif

#if NETCORE
    /// <summary>
    /// Platform-specific version of action result.
    /// </summary>
    public interface ITestActionResult : IActionResult { }

    /// <summary>
    /// Wrapper for platform-specific version of action result.
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
        private IActionResult innerResult;

        public TestActionResult(IActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            return innerResult.ExecuteResultAsync(context);
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of object result.
    /// </summary>
    public class TestObjectResult : ObjectResult, ITestActionResult
    {
        public TestObjectResult(object innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of status code result.
    /// </summary>
    public class TestStatusCodeResult : StatusCodeResult, ITestActionResult
    {
        private StatusCodeResult innerResult;

        public TestStatusCodeResult(StatusCodeResult innerResult)
            :base(innerResult.StatusCode)
        {
            this.innerResult = innerResult;
        }
    }
#else
    /// <summary>
    /// Platform-specific version of action result.
    /// </summary>
    public interface ITestActionResult : IHttpActionResult { }

    /// <summary>
    /// Wrapper for platform-specific version of action result.
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
        protected IHttpActionResult innerResult;

        public TestActionResult(IHttpActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        protected TestActionResult()
        {
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return innerResult.ExecuteAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Wrapper for platform-specific version of status code result.
    /// </summary>
    public class TestStatusCodeResult : TestActionResult
    {
        public TestStatusCodeResult(StatusCodeResult innerResult)
            : base(innerResult)
        {
        }
    }
#endif

    /// <summary>
    /// Platform-agnostic version of HttpMethod attributes. AspNetCore attributes are not sealed
    /// so they are used as a base class. AspNet has sealed attributes so the code is copied.
    /// </summary>
#if NETCORE
    /// <summary>
    /// Platform-agnostic version of IActionHttpMethodProviders.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpDeleteAttribute : Microsoft.AspNetCore.Mvc.HttpDeleteAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : Microsoft.AspNetCore.Mvc.HttpGetAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPatchAttribute : Microsoft.AspNetCore.Mvc.HttpPatchAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPostAttribute : Microsoft.AspNetCore.Mvc.HttpPostAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPutAttribute : Microsoft.AspNetCore.Mvc.HttpPutAttribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private int? _order;
        public AcceptVerbsAttribute(params string[] methods)
        {
            HttpMethods = methods.Select(method => method.ToUpperInvariant());
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; }

        /// <inheritdoc />
        public string Route { get; set; }

        /// <inheritdoc />
        string IRouteTemplateProvider.Template => Route;

        /// <inheritdoc />
        public int Order
        {
            get { return _order ?? 0; }
            set { _order = value; }
        }

        /// <inheritdoc />
        int? IRouteTemplateProvider.Order => _order;

        /// <inheritdoc />
        public string Name { get; set; }
    }
#else
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpDeleteAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Delete }); }
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpGetAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Get }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPatchAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { new HttpMethod("PATCH") }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Post }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPutAttribute : Attribute, IActionHttpMethodProvider
    {
        public Collection<HttpMethod> HttpMethods
        {
            get { return new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Put }); }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider
    {
        public AcceptVerbsAttribute(params string[] methods)
        {
            HttpMethods = new Collection<HttpMethod>(methods.Select(s => new HttpMethod(s)).ToList());
        }

        public Collection<HttpMethod> HttpMethods { get; }
    }
#endif

    /// <summary>
    /// Platform-agnostic version of formatting attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
#if NETCORE
    public class FromBodyAttribute : Microsoft.AspNetCore.Mvc.FromBodyAttribute { }
#else
    public sealed class FromBodyAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            IEnumerable<MediaTypeFormatter> formatters = parameter.Configuration.Formatters;
            IBodyModelValidator validator = parameter.Configuration.Services.GetBodyModelValidator();
            return parameter.BindWithFormatter(formatters, validator);
        }
    }
#endif

    /// <summary>
    /// An attribute that specifies that the value can be bound from the query string or route data.
    /// </summary>
#if NETCORE
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromUriAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        private static readonly BindingSource FromUriSource = CompositeBindingSource.Create(
            new BindingSource[] { BindingSource.Path, BindingSource.Query },
            "Custom.BindingSource_URL");

        /// <inheritdoc />
        public BindingSource BindingSource { get { return FromUriSource; } }

        /// <inheritdoc />
        public string Name { get; set; }
    }
#endif

    /// <summary>
    /// Represents an <see cref="IQueryable"/> containing zero or one entities. Use together with an
    /// <c>[EnableQuery]</c> from the System.Web.Http.OData or System.Web.OData namespace.
    /// </summary>
    public sealed class TestSingleResult : SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestSingleResult"/> class.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> containing zero or one entities.</param>
        public TestSingleResult(IQueryable queryable)
            : base(queryable)
        {
        }

        /// <summary>
        /// Creates a System.Web.Http.SingleResult`1 from an System.Linq.IQueryable`1
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="queryable">The System.Linq.IQueryable`1 containing zero or one entities.</param>
        /// <returns>The created TestSingleResult.</returns>
        public static new TestSingleResult<T> Create<T>(IQueryable<T> queryable)
        {
            return new TestSingleResult<T>(queryable);
        }
    }

    /// <summary>
    /// Represents an <see cref="IQueryable{T}"/> containing zero or one entities. Use together with an
    /// <c>[EnableQuery]</c> from the System.Web.Http.OData or System.Web.OData namespace.
    /// </summary>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    public sealed class TestSingleResult<T> : SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResult{T}"/> class.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}"/> containing zero or one entities.</param>
        public TestSingleResult(IQueryable<T> queryable)
            : base(queryable)
        {
        }

        /// <summary>
        /// The <see cref="IQueryable{T}"/> containing zero or one entities.
        /// </summary>
        public new IQueryable<T> Queryable
        {
            get
            {
                return base.Queryable as IQueryable<T>;
            }
        }
    }
}
