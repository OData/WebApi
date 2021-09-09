//-----------------------------------------------------------------------------
// <copyright file="IWebApiActionDescriptor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Provides information about the action methods.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiActionDescriptor
    {
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        string ControllerName { get; }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        string ActionName { get; }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute;

        /// <summary>
        /// Returns the <see cref="MethodInfo"/> representing the controller action.
        /// </summary>
        /// <returns>The <see cref="MethodInfo"/> representing the controller action.</returns>
        MethodInfo GetMethodInfo();

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        /// <param name="method">Method to test.</param>
        bool IsHttpMethodSupported(ODataRequestMethod method);
    }
}
