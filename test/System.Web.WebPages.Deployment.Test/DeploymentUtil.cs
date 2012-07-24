// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;

namespace System.Web.WebPages.Deployment.Test
{
    internal static class DeploymentUtil
    {
        public static string GetBinDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Path.Combine(tempDirectory, "bin");
        }
    }
}
