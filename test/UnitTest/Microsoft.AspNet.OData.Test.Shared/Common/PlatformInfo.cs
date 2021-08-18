//-----------------------------------------------------------------------------
// <copyright file="PlatformInfo.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Test.Common
{
    /// <summary>
    /// Used to retrieve the currently running platform.
    /// </summary>
    public static class PlatformInfo
    {
        private const string _net45TypeName = "System.IWellKnownStringEqualityComparer, mscorlib, Version=4.0.0.0, PublicKeyToken=b77a5c561934e089";
        private static Lazy<Platform> _platform = new Lazy<Platform>(GetPlatform, isThreadSafe: true);

        /// <summary>
        /// Gets the platform that the unit test is currently running on.
        /// </summary>
        public static Platform Platform
        {
            get { return _platform.Value; }
        }

        private static Platform GetPlatform()
        {
            if (Type.GetType(_net45TypeName, throwOnError: false) != null)
            {
                return Platform.Net45;
            }

            return Platform.Net40;
        }
    }
}
