// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;

namespace System.Web.Http.WebHost
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This is no longer needed and should be removed.", true)]
    public static class PreApplicationStartCode
    {
        private static bool _startWasCalled;

        public static void Start()
        {
            // Guard against multiple calls. All Start calls are made on same thread, so no lock needed here
            if (_startWasCalled)
            {
                return;
            }
            _startWasCalled = true;

            // We need to keep this for App-Compat reasons. Prior to 4.5 we registered a module here.
        }
    }
}
