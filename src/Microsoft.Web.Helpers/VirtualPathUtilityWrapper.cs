// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web;

namespace Microsoft.Web.Helpers
{
    internal sealed class VirtualPathUtilityWrapper : VirtualPathUtilityBase
    {
        public override string Combine(string basePath, string relativePath)
        {
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public override string ToAbsolute(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }
    }
}
