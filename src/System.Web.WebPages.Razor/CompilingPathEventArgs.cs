// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.WebPages.Razor
{
    public class CompilingPathEventArgs : EventArgs
    {
        public CompilingPathEventArgs(string virtualPath, WebPageRazorHost host)
        {
            VirtualPath = virtualPath;
            Host = host;
        }

        public string VirtualPath { get; private set; }
        public WebPageRazorHost Host { get; set; }
    }
}
