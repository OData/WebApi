// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Web.Helpers
{
    public abstract class VirtualPathUtilityBase
    {
        public abstract string Combine(string basePath, string relativePath);

        public abstract string ToAbsolute(string virtualPath);
    }
}
