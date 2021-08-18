//-----------------------------------------------------------------------------
// <copyright file="WebApiActionDescriptor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc.Internal;
#endif

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi action description to OData WebApi.
    /// </summary>
    internal class WebApiActionDescriptor : IWebApiActionDescriptor
    {
        /// <summary>
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        private IList<ODataRequestMethod> supportedHttpMethods;

        /// <summary>
        /// Gets the collection of supported HTTP methods for conventions.
        /// </summary>
        private static readonly string[] SupportedHttpMethodConventions = new string[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };

        /// <summary>
        /// The inner action wrapped by this instance.
        /// </summary>
        private ControllerActionDescriptor innerDescriptor;

        /// <summary>
        /// Initializes a new instance of the WebApiActionDescriptor class.
        /// </summary>
        /// <param name="actionDescriptor">The inner descriptor.</param>
        public WebApiActionDescriptor(ControllerActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            this.innerDescriptor = actionDescriptor;
            this.supportedHttpMethods = new List<ODataRequestMethod>();

            // Determine the supported methods.
            IEnumerable<string> actionMethods = actionDescriptor.ActionConstraints?
                .OfType<HttpMethodActionConstraint>()
                .FirstOrDefault()?
                .HttpMethods;

            if (actionMethods == null)
            {
                // If no HttpMethodActionConstraint is specified, fall back to convention the way AspNet does.
                actionMethods = SupportedHttpMethodConventions
                    .Where(method => actionDescriptor.MethodInfo.Name.StartsWith(method, StringComparison.OrdinalIgnoreCase));

                // Use POST as the default method.
                if (!actionMethods.Any())
                {
                    actionMethods = new string[] { "POST" };
                }
            }

            foreach (string method in actionMethods)
            {
                bool ignoreCase = true;
                ODataRequestMethod methodEnum = ODataRequestMethod.Unknown;
                if (Enum.TryParse<ODataRequestMethod>(method, ignoreCase, out methodEnum))
                {
                    this.supportedHttpMethods.Add(methodEnum);
                }
            }
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get { return this.innerDescriptor.ControllerName; }
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string ActionName
        {
            get { return this.innerDescriptor.ActionName; }
        }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return this.innerDescriptor.MethodInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        public bool IsHttpMethodSupported(ODataRequestMethod method)
        {
            return this.supportedHttpMethods.Contains(method);
        }
    }
}
