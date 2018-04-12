// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.AspNet.OData
{
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
        public new TestBadRequestResult BadRequest() { return new TestBadRequestResult(base.BadRequest()); }

        [NonAction]
        public new TestBadRequestObjectResult BadRequest(object error) { return new TestBadRequestObjectResult(base.BadRequest(error)); }

        [NonAction]
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }

        [NonAction]
        public new TestCreatedObjectResult<TEntity> Created<TEntity>(TEntity value)
        {
            return new TestCreatedObjectResult<TEntity>(new CreatedODataResult<TEntity>(value));
        }

        [NonAction]
        public new TestUpdatedObjectResult<TEntity> Updated<TEntity>(TEntity value)
        {
            return new TestUpdatedObjectResult<TEntity>(new UpdatedODataResult<TEntity>(value));
        }
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
    /// Wrapper for BadRequestResult
    /// </summary>
    public class TestBadRequestResult : TestActionResult
    {
        public TestBadRequestResult(BadRequestResult innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for BadRequestObjectResult
    /// </summary>
    public class TestBadRequestObjectResult : TestActionResult
    {
        private object _value;

        public TestBadRequestObjectResult(BadRequestObjectResult innerResult)
            : base(innerResult)
        {
            _value = innerResult.Value;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (_value is string)
            {
                context.HttpContext.Response.StatusCode = 400;
                context.HttpContext.Response.WriteAsync(_value as string);
            }
            else
            {
                base.ExecuteResultAsync(context);
            }

            return Task.CompletedTask;
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
    /// Wrapper for UpdatedResult
    /// </summary>
    public class TestUpdatedObjectResult<T> : TestActionResult
    {
        public TestUpdatedObjectResult(UpdatedODataResult<T> innerResult)
            : base(innerResult)
        {
        }
    }

    /// <summary>
    /// Wrapper for OkObjectResult
    /// </summary>
    public class TestOkObjectResult : TestObjectResult
    {
        public TestOkObjectResult(object innerResult)
            : base(innerResult)
        {
            this.StatusCode = 200;
        }
    }

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

        public virtual Task ExecuteResultAsync(ActionContext context)
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
}
