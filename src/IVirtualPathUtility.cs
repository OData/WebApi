// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Internal.Web.Utils
{
    internal interface IVirtualPathUtility
    {
        string Combine(string basePath, string relativePath);

        string ToAbsolute(string virtualPath);
    }
}
