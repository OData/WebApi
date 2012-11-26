// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.Mvc.Facebook
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class FacebookAuthorizeAttribute : Attribute
    {
        private ReadOnlyCollection<string> _permissions;

        public FacebookAuthorizeAttribute()
            : this(new string[0])
        {
        }

        public FacebookAuthorizeAttribute(params string[] permissions)
        {
            _permissions = new ReadOnlyCollection<string>(permissions);
        }

        public ReadOnlyCollection<string> Permissions
        {
            get
            {
                return _permissions;
            }
        }
    }
}
