// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Results
{
    /// <summary>
    /// Defines a contract that represents the result of an inner action result.
    /// </summary>
    public interface IInnerActionResult
    {
        /// <summary>
        /// Gets the inner action result.
        /// </summary>
        /// <param name="request">The HttpRequest.</param>
        /// <returns>The Inner action result.</returns>
        IActionResult GetInnerActionResult(HttpRequest request);
    }
}
