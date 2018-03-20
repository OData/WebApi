// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;
#else
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
#endif

namespace Microsoft.Test.AspNet.OData
{
    /// <summary>
    /// NonODataController is an abstracted ASP.NET [Core] controller.
    /// </summary>
#if NETCORE
    public class TestNonODataController : Controller
#else
    public class TestNonODataController : ApiController
#endif
    {
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
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
#if NETCORE
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }
#else
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
#endif

        [NonAction]
#if NETCORE
        public new TestCreatedObjectResult<TEntity> Created<TEntity>(TEntity value)
        {
            return new TestCreatedObjectResult<TEntity>(new CreatedODataResult<TEntity>(value));
        }
#else
        public new TestCreatedObjectResult<TEntity> Created<TEntity>(TEntity value)
        {
            return new TestCreatedObjectResult<TEntity>(base.Created<TEntity>(value));
        }
#endif
    }

    /// <summary>
    /// Wrapper for NotFoundResult
    /// </summary>
    public class TestNotFoundResult : TestActionResult
    {
        public TestNotFoundResult(NotFoundResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for OkResult
    /// </summary>
    public class TestOkResult : TestActionResult
    {
        public TestOkResult(OkResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for CreatedResult
    /// </summary>
    public class TestCreatedObjectResult<T> : TestActionResult
    {
        public TestCreatedObjectResult(CreatedODataResult<T> innerResult)
            : base(innerResult)
        {
        }
    }

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
#else
    public class TestOkObjectResult<T> : TestActionResult
    {
        public TestOkObjectResult(OkNegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }
    }
#endif

#if NETCORE
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    public interface ITestActionResult : IActionResult { }

    /// <summary>
    /// Wrapper for platform-agnostic version of action result.
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
    /// Wrapper for platform-agnostic version of object result.
    /// </summary>
    public class TestObjectResult : ObjectResult, ITestActionResult
    {
        public TestObjectResult(object innerResult)
            : base(innerResult)
        {
        }
    }
#else
    /// <summary>
    /// Platform-agnostic version of action result.
    /// </summary>
    public interface ITestActionResult : IHttpActionResult { }

    /// <summary>
    /// Wrapper for platform-agnostic version of action result.
    /// </summary>
    public class TestActionResult : ITestActionResult
    {
        private IHttpActionResult innerResult;

        public TestActionResult(IHttpActionResult innerResult)
        {
            this.innerResult = innerResult;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return innerResult.ExecuteAsync(cancellationToken);
        }

    }
#endif
}
