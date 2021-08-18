//-----------------------------------------------------------------------------
// <copyright file="ODataUriFunctions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// OData UriFunctions helper.
    /// </summary>
    public static class ODataUriFunctions
    {
        /// <summary>
        /// This is a shortcut of adding the custom FunctionSignature through 'CustomUriFunctions' class and
        /// binding the function name to it's MethodInfo through 'UriFunctionsBinder' class.
        /// See these classes documentations.
        /// In case of an exception, both operations(adding the signature and binding the function) will be undone.
        /// </summary>
        /// <param name="functionName">The uri function name that appears in the OData request uri.</param>
        /// <param name="functionSignature">The new custom function signature.</param>
        /// <param name="methodInfo">The MethodInfo to bind the given function name.</param>
        /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.AddCustomUriFunction' and 'UriFunctionBinder.BindUriFunctionName' methods.</exception>
        public static void AddCustomUriFunction(string functionName,
            FunctionSignatureWithReturnType functionSignature, MethodInfo methodInfo)
        {
            try
            {
                // Add to OData.Libs function signature
                CustomUriFunctions.AddCustomUriFunction(functionName, functionSignature);

                // Bind the method to it's MethoInfo 
                UriFunctionsBinder.BindUriFunctionName(functionName, methodInfo);
            }
            catch
            {
                // Clear in case of excpetion
                RemoveCustomUriFunction(functionName, functionSignature, methodInfo);
                throw;
            }
        }

        /// <summary>
        /// This is a shortcut of removing the FunctionSignature through 'CustomUriFunctions' class and
        /// unbinding the function name from it's MethodInfo through 'UriFunctionsBinder' class.
        /// See these classes documentations.
        /// </summary>
        /// <param name="functionName">The uri function name that appears in the OData request uri.</param>
        /// <param name="functionSignature">The new custom function signature.</param>
        /// <param name="methodInfo">The MethodInfo to bind the given function name.</param>
        /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.RemoveCustomUriFunction' and 'UriFunctionsBinder.UnbindUriFunctionName' methods.</exception>
        /// <returns>'True' if the fucntion signature has successfully removed and unbinded. 'False' otherwise.</returns>
        public static bool RemoveCustomUriFunction(string functionName,
            FunctionSignatureWithReturnType functionSignature, MethodInfo methodInfo)
        {
            return
                CustomUriFunctions.RemoveCustomUriFunction(functionName, functionSignature) &&
                UriFunctionsBinder.UnbindUriFunctionName(functionName, methodInfo);
        }
    }
}
