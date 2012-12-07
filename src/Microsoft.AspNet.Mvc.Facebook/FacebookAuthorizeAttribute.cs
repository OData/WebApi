// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.Mvc.Facebook
{
    /// <summary>
    /// Restricts the access to requests with valid Facebook signed request parameter and to users that have the required permissions.
    /// This attribute can be declared on a controller, an action or both.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class FacebookAuthorizeAttribute : Attribute
    {
        private ReadOnlyCollection<string> _permissions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeAttribute" /> class without requiring permissions.
        /// </summary>
        public FacebookAuthorizeAttribute()
            : this(new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookAuthorizeAttribute" /> class requiring permissions.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        public FacebookAuthorizeAttribute(params string[] permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            _permissions = new ReadOnlyCollection<string>(permissions);
        }

        /// <summary>
        /// Gets the required permissions.
        /// </summary>
        public ReadOnlyCollection<string> Permissions
        {
            get
            {
                return _permissions;
            }
        }
    }
}