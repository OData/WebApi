// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ActionLinkAreaAttribute : Attribute
    {
        public ActionLinkAreaAttribute(string area)
        {
            if (area == null)
            {
                throw new ArgumentNullException("area");
            }

            Area = area;
        }

        public string Area { get; private set; }
    }
}
