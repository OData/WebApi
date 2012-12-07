// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Configuration;
using System.Web;

namespace Microsoft.AspNet.Mvc.Facebook
{
    /// <summary>
    /// Code that will be executed before application start.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        private static bool _startWasCalled;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public static void Start()
        {
            if (!_startWasCalled)
            {
                _startWasCalled = true;
                if (!DisableAuthenticationModule())
                {
                    HttpApplication.RegisterModule(typeof(FacebookAuthenticationModule));
                }
            }
        }

        private static bool DisableAuthenticationModule()
        {
            string settingValue = ConfigurationManager.AppSettings[FacebookAppSettingKeys.DisableAuthenticationModule];
            bool excludeAuthenticationModule;
            if (!String.IsNullOrEmpty(settingValue) && Boolean.TryParse(settingValue, out excludeAuthenticationModule))
            {
                return excludeAuthenticationModule;
            }
            return false;
        }
    }
}