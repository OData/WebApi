// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.WebPages.Instrumentation
{
    internal partial class HttpContextAdapter
    {
        private static readonly bool _isInstrumentationAvailable = typeof(HttpContext).GetProperty("PageInstrumentation", BindingFlags.Instance | BindingFlags.Public) != null;

        internal static bool IsInstrumentationAvailable
        {
            get { return _isInstrumentationAvailable; }
        }
    }
}
