// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an Collection<HttpMethod>.")]
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider
    {
        private readonly Collection<HttpMethod> _httpMethods;

        public AcceptVerbsAttribute(params string[] methods)
        {
            _httpMethods = methods != null
                                   ? new Collection<HttpMethod>(methods.Select(method => HttpMethodHelper.GetHttpMethod(method)).ToArray())
                                   : new Collection<HttpMethod>(new HttpMethod[0]);
        }

        internal AcceptVerbsAttribute(params HttpMethod[] methods)
        {
            _httpMethods = new Collection<HttpMethod>(methods);
        }

        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }
    }
}
