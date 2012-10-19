// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Configuration;

namespace Microsoft.AspNet.Mvc.Facebook.Services
{
    public static class FacebookSettings
    {
        private static string _facebookAppUrl = "https://apps.facebook.com";

        static FacebookSettings()
        {
            DefaultUserStorageService = DefaultFacebookUserStorageService.Instance;
            DefaultObjectStorageService = DefaultFacebookObjectStorageService.Instance;
        }

        public static string AppId { get; set; }
        public static string AppSecret { get; set; }
        public static string AppNamespace { get; set; }
        public static string RealtimeCallbackUrl { get; set; }

        //TODO: (ErikPo) Move these to some place better

        public static IFacebookUserStorageService DefaultUserStorageService { get; set; }
        public static IFacebookObjectStorageService DefaultObjectStorageService { get; set; }
        public static string FacebookAppUrl
        {
            get
            {
                return _facebookAppUrl;
            }
            set
            {
                _facebookAppUrl = value;
            }
        }

        public static void LoadFromConfig()
        {
            AppId = ConfigurationManager.AppSettings["Facebook.AppId"];
            AppSecret = ConfigurationManager.AppSettings["Facebook.AppSecret"];
            AppNamespace = ConfigurationManager.AppSettings["Facebook.AppNamespace"];
            RealtimeCallbackUrl = ConfigurationManager.AppSettings["Facebook.RealtimeCallbackUrl"];
            FacebookAppUrl = ConfigurationManager.AppSettings["Facebook.FacebookAppUrl"] ?? _facebookAppUrl;
        }
    }
}
