// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "base class for HttpGet and other attributes.")]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an ICollection<string>.")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AcceptVerbsAttribute : ActionMethodSelectorAttribute
    {
        private readonly HttpVerbsValidator _httpVerbsValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(HttpVerbs verbs)
        {
            _httpVerbsValidator = new HttpVerbsValidator(verbs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="verbs">The HTTP methods the action supports.</param>
        public AcceptVerbsAttribute(params string[] verbs)
        {
            _httpVerbsValidator = new HttpVerbsValidator(verbs);
        }      

        /// <summary>
        /// Gets the set of allowed HTTP methods specified by this attribute. 
        /// </summary>
        public ICollection<string> Verbs
        {
            get { return _httpVerbsValidator.Verbs; }
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return _httpVerbsValidator.IsValidForRequest(controllerContext);
        }
    }
}
