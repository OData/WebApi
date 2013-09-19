// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Deployment.Test
{
    public static class LatestRazorVersion
    {
        public static readonly Version LatestVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Razor", typeof(ParserResults));
    }
}
