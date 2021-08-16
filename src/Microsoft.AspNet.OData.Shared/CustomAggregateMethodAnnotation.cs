//-----------------------------------------------------------------------------
// <copyright file="CustomAggregateMethodAnnotation.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Allows client to tell OData which are the custom aggregation methods defined.
    /// In order to do it, it must receive a methodToken - that is the full identifier
    /// of the method in the OData URL - and an IDictionary that maps the input type
    /// of the aggregation method to its MethodInfo.
    /// </summary>
    public class CustomAggregateMethodAnnotation
    {
        private readonly Dictionary<string, IDictionary<Type, MethodInfo>> _tokenToMethodMap
            = new Dictionary<string, IDictionary<Type, MethodInfo>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAggregateMethodAnnotation"/> class.
        /// </summary>
        public CustomAggregateMethodAnnotation()
        {
        }

        /// <summary>
        /// Adds all implementations of a method that share the same methodToken.
        /// </summary>
        /// <param name="methodToken">The given method token.</param>
        /// <param name="methods">The given method dictionary.</param>
        /// <returns></returns>
        public CustomAggregateMethodAnnotation AddMethod(string methodToken, IDictionary<Type, MethodInfo> methods)
        {
            _tokenToMethodMap.Add(methodToken, methods);
            return this;
        }

        /// <summary>
        /// Get an implementation of a method with the specifies returnType and methodToken.
        /// If there's no method that matches the requirements, returns null.
        /// </summary>
        /// <param name="methodToken">The given method token.</param>
        /// <param name="returnType">The given return type.</param>
        /// <param name="methodInfo">The output of method info.</param>
        /// <returns>True if the method info was found, false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out param is appropriate here")]
        public bool GetMethodInfo(string methodToken, Type returnType, out MethodInfo methodInfo)
        {
            IDictionary<Type, MethodInfo> methodWrapper;
            methodInfo = null;

            if (_tokenToMethodMap.TryGetValue(methodToken, out methodWrapper))
            {
                return methodWrapper.TryGetValue(returnType, out methodInfo);
            }

            return false;
        }
    }
}
