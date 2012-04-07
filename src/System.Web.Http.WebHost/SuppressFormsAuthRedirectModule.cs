// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Security;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Recovers the response status code to 401 if it was altered by <see cref="T:System.Web.Security.FormsAuthenticationModule"/>.
    /// This module autoregisters on .NET 4.0, which ensures it runs after <see cref="T:System.Web.Security.FormsAuthenticationModule"/>.
    /// </summary>
    internal class SuppressFormsAuthRedirectModule : IHttpModule
    {
        internal static readonly string SuppressFormsAuthenticationRedirectPropertyName = "SuppressFormsAuthenticationRedirect";

        internal static readonly string AppSettingsSuppressFormsAuthenticationRedirectKey = "webapi:EnableSuppressRedirect";

        internal static readonly object DisableAuthenticationRedirectKey = new Object();

        /// <summary>
        /// Abstract the properties needed by <see cref="T:System.Web.Http.WebHost.SuppressFormsAuthRedirectModule"/> for unit testing purposes.
        /// </summary>
        internal interface IDisableRedirect
        {
            IDictionary ContextItems { get; }
            HttpResponse Response { get; }
        }

        /// <summary>
        /// Enables authentication redirects.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context.</param>
        internal static void AllowAuthenticationRedirect(HttpContextBase httpContextBase)
        {
            SetDisableAuthenticationRedirectState(httpContextBase.Items, value: false);
        }

        /// <summary>
        /// Disables authentication redirects.
        /// </summary>
        /// <param name="httpContextBase">The HTTP context.</param>
        internal static void DisableAuthenticationRedirect(HttpContextBase httpContextBase)
        {
            SetDisableAuthenticationRedirectState(httpContextBase.Items, value: true);
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object source, EventArgs args)
        {
            HttpApplication httpApplication = source as HttpApplication;

            if (httpApplication == null)
            {
                return;
            }

            EnsureRestoreUnauthorized(new HttpApplicationDisableRedirect(httpApplication));
        }

        internal static void EnsureRestoreUnauthorized(IDisableRedirect disableRedirect)
        {
            Contract.Assert(disableRedirect != null);

            HttpResponse response = disableRedirect.Response;

            // If there was no redirection, do nothing
            if (response.StatusCode != (int)HttpStatusCode.Redirect)
            {
                return;
            }

            // If the flag is set and is true, revert the redirection
            if (disableRedirect.ContextItems.Contains(DisableAuthenticationRedirectKey)
                && Convert.ToBoolean(disableRedirect.ContextItems[DisableAuthenticationRedirectKey], CultureInfo.InvariantCulture))
            {
                response.TrySkipIisCustomErrors = true;
                response.ClearContent();
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.RedirectLocation = null;
            }
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Registers the module if necessary.
        /// </summary>
        /// <remarks>
        /// We do not want the module to be registered if:
        /// - Running on .NET 4.5 because there is a standard way to prevent the redirection
        /// - The behavior is explicitly disabled using the appSettings flag
        /// - The module <see cref="T:System.Web.Security.FormsAuthenticationModule"/> is not enabled
        /// </remarks>
        public static void Register()
        {
            // If FormsAuthentication is not enabled, this module is not needed
            if (!FormsAuthentication.IsEnabled)
            {
                return;
            }

            // If explicitly requested, don't enable the module
            if (!GetEnabled(WebConfigurationManager.AppSettings))
            {
                return;
            }

            PropertyInfo suppressRedirect = typeof(HttpResponseBase).GetProperty(SuppressFormsAuthenticationRedirectPropertyName, BindingFlags.Instance | BindingFlags.Public);

            // Don't enable the module if hosted on .NET 4.5 or later. In this case the automatic 
            // redirection will be disabled using the specific API call
            if (suppressRedirect != null)
            {
                return;
            }

            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(SuppressFormsAuthRedirectModule));
        }

        /// <summary>
        /// Returns whether the module is explicitly enabled or not
        /// </summary>
        internal static bool GetEnabled(NameValueCollection appSettings)
        {
            string disableSuppressRedirect = appSettings.Get(AppSettingsSuppressFormsAuthenticationRedirectKey);

            if (!String.IsNullOrEmpty(disableSuppressRedirect))
            {
                bool enabled;

                // anything but "false" will return true, which is the default behavior
                if (Boolean.TryParse(disableSuppressRedirect, out enabled))
                {
                    if (!enabled)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void SetDisableAuthenticationRedirectState(IDictionary items, bool value)
        {
            items[DisableAuthenticationRedirectKey] = value;
        }

        /// <summary>
        /// Wrapper implementation of <see cref="T:System.Web.Http.WebHost.SuppressFormsAuthRedirectModule.IDisableRedirect"/> for <see cref="T:System.Web.HttpApplication"/>.
        /// </summary>
        internal class HttpApplicationDisableRedirect : IDisableRedirect
        {
            private HttpApplication _httpApplication;

            public HttpApplicationDisableRedirect(HttpApplication httpApplication)
            {
                Contract.Assert(httpApplication != null);

                _httpApplication = httpApplication;
            }

            public IDictionary ContextItems
            {
                get { return _httpApplication.Context.Items; }
            }

            public HttpResponse Response
            {
                get { return _httpApplication.Response; }
            }
        }
    }
}