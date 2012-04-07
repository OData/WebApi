// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    internal sealed class NullViewLocationCache : IViewLocationCache
    {
        #region IViewLocationCache Members

        public string GetViewLocation(HttpContextBase httpContext, string key)
        {
            return null;
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
        {
        }

        #endregion
    }
}
