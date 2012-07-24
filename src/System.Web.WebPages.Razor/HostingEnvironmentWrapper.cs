// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Hosting;

namespace System.Web.WebPages.Razor
{
    internal sealed class HostingEnvironmentWrapper : IHostingEnvironment
    {
        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}
