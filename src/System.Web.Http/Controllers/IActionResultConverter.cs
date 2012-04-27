// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// A contract for a conversion routine that can take the result of an action returned from
    /// <see cref="HttpActionDescriptor.ExecuteAsync(HttpControllerContext, IDictionary{string, object}, CancellationToken)"/>
    /// and convert it to an instance of <see cref="HttpResponseMessage"/>.
    /// </summary>
    public interface IActionResultConverter
    {
        HttpResponseMessage Convert(HttpControllerContext controllerContext, object actionResult);
    }
}