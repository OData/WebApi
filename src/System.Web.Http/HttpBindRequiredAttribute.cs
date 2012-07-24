// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.ModelBinding;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HttpBindRequiredAttribute : HttpBindingBehaviorAttribute
    {
        public HttpBindRequiredAttribute()
            : base(HttpBindingBehavior.Required)
        {
        }
    }
}
