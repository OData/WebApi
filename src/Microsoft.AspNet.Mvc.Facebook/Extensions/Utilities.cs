// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
#if Debug
namespace Microsoft.AspNet.Mvc.Facebook.Extensions
{
    internal static class Utilities
    {
        public static void Log(string data)
        {
            System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Log.txt", "\r\n" + data);
        }
    }
}
#endif