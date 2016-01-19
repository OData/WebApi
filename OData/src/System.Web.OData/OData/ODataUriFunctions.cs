using Microsoft.OData.Core.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData
{
    /// <summary>
    /// OData UriFunctions helper.
    /// </summary>
    public static class ODataUriFunctions
    {
        /// <summary>
        /// This is a shortcut of adding the custom FunctionSignature through 'CustomUriFunctions' class and
        /// binding the function name to it's MethodInfo through 'UriFunctionsToClrBinder' class.
        /// See these classes documentations.
        /// In case of an excpetion, both operations(adding the signature and binding the function) will be undone.
        /// </summary>
        /// <param name="customFunctionName">The uri function name that appears in the OData request uri.</param>
        /// <param name="functionSignature">The new custom function signature.</param>
        /// <param name="methoInfo">The MethodInfo to bind the given function name.</param>
        /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.AddCustomUriFunction' and 'UriFunctionsToClrBinder.BindUriFunctionName' methods.</exception>
        public static void AddUriCustomFunction(string customFunctionName, FunctionSignatureWithReturnType functionSignature, MethodInfo methoInfo)
        {
            try
            {
                // Add to OData.Libs function signature
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, functionSignature);

                // Bind the method to it's MethoInfo 
                UriFunctionsToClrBinder.BindUriFunctionName(customFunctionName, methoInfo);
            }
            catch
            {
                // Clear in case of excpetion
                RemoveCustomUriFunction(customFunctionName, functionSignature, methoInfo);
                throw;
            }
        }

        /// <summary>
        /// This is a shortcut of removing the FunctionSignature through 'CustomUriFunctions' class and
        /// unbinding the function name from it's MethodInfo through 'UriFunctionsToClrBinder' class.
        /// See these classes documentations.
        /// </summary>
        /// <param name="customFunctionName">The uri function name that appears in the OData request uri.</param>
        /// <param name="functionSignature">The new custom function signature.</param>
        /// <param name="methoInfo">The MethodInfo to bind the given function name.</param>
        /// <exception cref="Exception">Any exception thrown by 'CustomUriFunctions.RemoveCustomUriFunction' and 'UriFunctionsToClrBinder.UnbindUriFunctionName' methods.</exception>
        /// <returns>'True' if the fucntion signature has successfully removed and unbinded. 'False' otherwise.</returns>
        public static bool RemoveCustomUriFunction(string customFunctionName, FunctionSignatureWithReturnType functionSignature, MethodInfo methoInfo)
        {
            return
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName, functionSignature) &&
                UriFunctionsToClrBinder.UnbindUriFunctionName(customFunctionName, methoInfo);
        }
    }
}
