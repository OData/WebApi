//-----------------------------------------------------------------------------
// <copyright file="TestODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Results;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Test.Abstraction
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
        public new TestOkResult Ok() { return new TestOkResult(base.Ok()); }

        [NonAction]
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }

        [NonAction]
        public new TestCreatedObjectResult<TEntity> Created<TEntity>(TEntity value)
        {
            return new TestCreatedObjectResult<TEntity>(base.Created<TEntity>(value));
        }

        [NonAction]
        public new TestUpdatedObjectResult<TEntity> Updated<TEntity>(TEntity value)
        {
            return new TestUpdatedObjectResult<TEntity>(base.Updated<TEntity>(value));
        }

        [NonAction]
        public TestNoContentResult NoContent() { return new TestNoContentResult(StatusCode(HttpStatusCode.NoContent)); }

        // Helper method for extracting key for the entity from a Uri
        public TKey GetKeyFromLinkUri<TKey>(HttpRequestMessage request, Uri link)
        {
            if (link == null)
                throw new ArgumentNullException("link");

            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);
            var serviceRoot = urlHelper.CreateODataLink(
                request.ODataProperties().RouteName,
                request.GetPathHandler(),
                new List<ODataPathSegment>()
            );
            var odataPath = request.GetPathHandler().Parse(
                serviceRoot,
                link.LocalPath,
                request.GetRequestContainer()
            );

            var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();

            if (keySegment == null || !keySegment.Keys.Any())
                throw new InvalidOperationException("This link does not contain a key.");

            // Return the key value of the first segment
            return (TKey)keySegment.Keys.First().Value;
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

    public class TestOkObjectResult<T> : TestActionResult
    {
        public TestOkObjectResult(OkNegotiatedContentResult<T> innerResult)
            : base(innerResult)
        {
        }
    }

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

    /// <summary>
    /// Wrapper for NoContentResult
    /// </summary>
    public class TestNoContentResult : TestActionResult
    {
        public TestNoContentResult(IHttpActionResult innerResult)
            : base(innerResult)
        {

        }
    }
}
