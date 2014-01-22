// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// Provides an abstraction for selecting a controller and an action for OData requests.
    /// </summary>
    public interface IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        string SelectController(ODataPath odataPath, HttpRequestMessage request);

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap);
    }
}