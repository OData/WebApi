// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Web.Security;

namespace WebMatrix.WebData
{
    internal static class ConfigUtil
    {
        private static bool _simpleMembershipEnabled = IsSimpleMembershipEnabled();
        private static string _loginUrl = GetLoginUrl();

        public static bool SimpleMembershipEnabled
        {
            get { return _simpleMembershipEnabled; }
        }

        public static string LoginUrl
        {
            get { return _loginUrl; }
        }

        private static string GetLoginUrl()
        {
            return ConfigurationManager.AppSettings[FormsAuthenticationSettings.LoginUrlKey] ??
                   (ShouldPreserveLoginUrl() ? FormsAuthentication.LoginUrl : FormsAuthenticationSettings.DefaultLoginUrl);
        }

        private static bool IsSimpleMembershipEnabled()
        {
            string settingValue = ConfigurationManager.AppSettings[WebSecurity.EnableSimpleMembershipKey];
            bool enabled;
            if (!String.IsNullOrEmpty(settingValue) && Boolean.TryParse(settingValue, out enabled))
            {
                return enabled;
            }
            // Simple Membership is enabled by default, but attempts to delegate to the current provider if not initialized.
            return true;
        }

        private static bool ShouldPreserveLoginUrl()
        {
            string settingValue = ConfigurationManager.AppSettings[FormsAuthenticationSettings.PreserveLoginUrlKey];
            bool preserveLoginUrl;
            if (!String.IsNullOrEmpty(settingValue) && Boolean.TryParse(settingValue, out preserveLoginUrl))
            {
                return preserveLoginUrl;
            }

            // For backwards compatible with WebPages 1.0, we override the loginUrl value if 
            // the PreserveLoginUrl key is not present.
            return false;
        }
    }
}
