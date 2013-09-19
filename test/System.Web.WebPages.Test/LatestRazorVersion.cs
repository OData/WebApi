// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public static class LatestRazorVersion
    {
        private static readonly Version LatestVersion = VersionTestHelper.GetVersionFromAssembly("System.Web.Razor", typeof(ParserResults));

        public static readonly string MajorMinor = LatestVersion.Major + "." + LatestVersion.Minor;
    }
}
