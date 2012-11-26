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

        public IFacebookStorageProvider StorageProvider { get; set; }

        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        public void LoadFromAppSettings()
        {
            AppId = ConfigurationManager.AppSettings["Facebook:AppId"];
            AppSecret = ConfigurationManager.AppSettings["Facebook:AppSecret"];
            AppNamespace = ConfigurationManager.AppSettings["Facebook:AppNamespace"];
            AppUrl = ConfigurationManager.AppSettings["Facebook:AppUrl"];
            AuthorizationRedirectPath = ConfigurationManager.AppSettings["Facebook:AuthorizationRedirectPath"];
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
