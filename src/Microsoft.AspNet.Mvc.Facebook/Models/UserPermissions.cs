// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    public class UserPermissions
    {
        public string Id { get; set; }
        public string Permissions { get; set; }

        internal IEnumerable<string> ParsePermissions()
        {
            if (Permissions == null)
            {
                return new string[0];
            }
            return Permissions.Split(',');
        }

        internal void SetPermissions(IEnumerable<string> permissions)
        {
            if (permissions == null)
            {
                throw new ArgumentNullException("value");
            }
            Permissions = String.Join(",", permissions);
        }
    }
}
