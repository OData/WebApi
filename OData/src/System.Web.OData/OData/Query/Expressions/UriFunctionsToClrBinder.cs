// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// This class helps to bind uri functions to CLR.
    /// For creating an Expression and apply it on a Queryable collection, we must get the CLR information,
    /// i.e MethodInfo of each EdmFunction which is mentioned in the EdmModel.
    /// If you add a custom uri function in OData.Core via 'CustomUriFunctions' class, you must bind it to it's MethodInfo.
    /// </summary>
    public static class UriFunctionsToClrBinder
    {
        #region Static Fields

        private static Dictionary<string, MethodInfo> MethodLiteralSignaturesToMethodInfo;

        private static object Locker;

        #endregion

        #region Ctor

        static UriFunctionsToClrBinder()
        {
            MethodLiteralSignaturesToMethodInfo = new Dictionary<string, MethodInfo>();
            Locker = new object();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Bind the given function name to the given MethodInfo.
        /// The binding helps to create an Expression out of the method.
        /// You can bind a static method, a static extension method, and an instance method.
        /// You should be carefull about binding instance methods - the declaring type of the method i.e the instance type, 
        /// will be considered as the first argument of the function.
        /// </summary>
        /// <param name="functionName">The uri function name that appears in the OData request uri.</param>
        /// <param name="methodInfo">The MethodInfo to bind the given function name.</param>
        /// <exception cref="ArgumentNullException">Function name argument is Null or empty</exception>
        /// <exception cref="ArgumentNullException">MethodInfo argument is Null</exception>
        /// <exception cref="ODataException">The given FunctionName is already binded to another MethodInfo.</exception>
        public static void BindUriFunctionName(string functionName, MethodInfo methodInfo)
        {
            // Validation
            if (string.IsNullOrEmpty(functionName))
            {
                throw Error.ArgumentNull("functionName");
            }

            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }

            // Get literal description of the method
            string methodLiteralSignature = GetMethodLiteralSignature(functionName, methodInfo);

            lock (Locker)
            {
                if (MethodLiteralSignaturesToMethodInfo.ContainsKey(methodLiteralSignature))
                {
                    throw new ODataException(string.Format(SRResources.UriFunctionClrBinderAlreadyBinded, methodLiteralSignature));
                }

                MethodLiteralSignaturesToMethodInfo.Add(methodLiteralSignature, methodInfo);
            }
        }

        /// <summary>
        /// Unbind the given function name from the given MethodInfo.
        /// </summary>
        /// <param name="functionName">The uri function name to unbind.</param>
        /// <param name="methodInfo">The MethodInfo to unbind from the given function name.</param>
        /// <exception cref="ArgumentNullException">Function name argument is Null or empty</exception>
        /// <exception cref="ArgumentNullException">MethodInfo argument is Null</exception>
        /// <returns>'True' if function has unbinded. 'False' otherwise.</returns>
        public static bool UnbindUriFunctionName(string functionName, MethodInfo methodInfo)
        {
            // Validation
            if (string.IsNullOrEmpty(functionName))
            {
                throw Error.ArgumentNull("functionName");
            }

            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }

            // Get literal description of the method
            string methodLiteralSignature = GetMethodLiteralSignature(functionName, methodInfo);

            lock (Locker)
            {
                return MethodLiteralSignaturesToMethodInfo.Remove(methodLiteralSignature);
            }
        }

        #endregion

        #region Internal Methods

        internal static bool TryGetMethodInfo(string functionName, IEnumerable<Type> methodArgumentsType, out MethodInfo methodInfo)
        {
            // Validation
            if (string.IsNullOrEmpty(functionName))
            {
                throw Error.ArgumentNull("functionName");
            }

            if (methodArgumentsType == null)
            {
                throw Error.ArgumentNull("methodArgumentsType");
            }

            // Get literal description of the method
            string methodLiteralSignature = GetMethodLiteralSignature(functionName, methodArgumentsType);

            lock (Locker)
            {
                return MethodLiteralSignaturesToMethodInfo.TryGetValue(methodLiteralSignature, out methodInfo);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a string describing the given method.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static string GetMethodLiteralSignature(string methodName, MethodInfo methodInfo)
        {
            // Get the arguments type of the given MethodInfo
            IEnumerable<Type> methodArgumentsType = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);

            // If method is not static - instance, the declaring type is the first argument. 
            if (!methodInfo.IsStatic)
            {
                methodArgumentsType = new Type[] { methodInfo.DeclaringType }.Concat(methodArgumentsType);
            }

            return GetMethodLiteralSignature(methodName, methodArgumentsType);
        }

        /// <summary>
        /// Creates a string describing the function signature.
        /// 'methodName(argTypeName1,argTypeName2,argTypeName3..)'
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodArgumentsType"></param>
        /// <returns></returns>
        private static string GetMethodLiteralSignature(string methodName, IEnumerable<Type> methodArgumentsType)
        {
            StringBuilder builder = new StringBuilder();
            string descriptionSeparator = "";
            builder.Append(descriptionSeparator);
            descriptionSeparator = "; ";

            string parameterSeparator = "";
            builder.Append(methodName);
            builder.Append('(');
            foreach (Type type in methodArgumentsType)
            {
                builder.Append(parameterSeparator);
                parameterSeparator = ", ";

                builder.Append(type.FullName);
            }

            builder.Append(')');

            return builder.ToString();
        }

        #endregion
    }
}
