//-----------------------------------------------------------------------------
// <copyright file="WebApiActionDescriptor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;

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
        /// The inner action wrapped by this instance.
        /// </summary>
        private HttpActionDescriptor innerDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiActionDescriptor"/> class.
        /// </summary>
        /// <param name="actionDescriptor">The inner descriptor.</param>
        public WebApiActionDescriptor(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            this.innerDescriptor = actionDescriptor;

            if (actionDescriptor.SupportedHttpMethods != null)
            {
                this.supportedHttpMethods = new List<ODataRequestMethod>();
                foreach (HttpMethod method in actionDescriptor.SupportedHttpMethods)
                {
                    bool ignoreCase = true;
                    ODataRequestMethod methodEnum = ODataRequestMethod.Unknown;
                    if (Enum.TryParse<ODataRequestMethod>(method.Method, ignoreCase, out methodEnum))
                    {
                        this.supportedHttpMethods.Add(methodEnum);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get
            {
                return this.innerDescriptor.ControllerDescriptor != null
                    ? this.innerDescriptor.ControllerDescriptor.ControllerName
                    : null;
            }
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
            return this.innerDescriptor.GetCustomAttributes<T>(inherit);
        }

        /// <inheritdoc/>
        public MethodInfo GetMethodInfo()
        {
            if (this.innerDescriptor is ReflectedHttpActionDescriptor actionDescriptor)
            {
                return actionDescriptor.MethodInfo;
            }

            return null;
        }

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        public bool IsHttpMethodSupported(ODataRequestMethod method)
        {
            if (this.supportedHttpMethods == null)
            {
                // Assume all methods are supported if not specified.
                return true;
            }

            return this.supportedHttpMethods.Contains(method);
        }
    }
}
