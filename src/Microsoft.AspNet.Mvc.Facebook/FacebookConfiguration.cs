// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using Microsoft.AspNet.Mvc.Facebook.Providers;

namespace Microsoft.AspNet.Mvc.Facebook
{
    public class FacebookConfiguration
    {
        private static readonly string FacebookAppBaseUrl = "https://apps.facebook.com";
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();
        private string _appUrl;

        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string AppNamespace { get; set; }

        public string AuthorizationRedirectPath { get; set; }

        public string AppUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_appUrl))
                {
                    _appUrl = GetAppUrl();
                }
                return _appUrl;
            }
            set
            {
                _appUrl = value;
            }
        }

        public IFacebookClientProvider ClientProvider { get; set; }

        public IFacebookPermissionService PermissionService { get; set; }

        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        public virtual void LoadFromAppSettings()
        {
            AppId = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppId];
            if (String.IsNullOrEmpty(AppId))
            {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AppSettingIsRequired,
                    FacebookAppSettingKeys.AppId));
            }

            AppSecret = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppSecret];
            if (String.IsNullOrEmpty(AppSecret))
            {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.AppSettingIsRequired,
                    FacebookAppSettingKeys.AppSecret));
            }

            AppNamespace = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppNamespace];
            AppUrl = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AppUrl];
            AuthorizationRedirectPath = ConfigurationManager.AppSettings[FacebookAppSettingKeys.AuthorizationRedirectPath];
        }

        private string GetAppUrl()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1}",
                FacebookAppBaseUrl,
                String.IsNullOrEmpty(AppNamespace) ? AppId : AppNamespace);
        }
    }
}