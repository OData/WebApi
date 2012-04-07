// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Microsoft.Web.WebPages.OAuth.Properties;
using WebMatrix.WebData;

namespace Microsoft.Web.WebPages.OAuth
{
    /// <summary>
    /// Contains APIs to manage authentication against OAuth &amp; OpenID service providers
    /// </summary>
    public static class OAuthWebSecurity
    {
        internal static IOpenAuthDataProvider OAuthDataProvider = new WebPagesOAuthDataProvider();

        // contains all registered authentication clients
        private static readonly AuthenticationClientCollection _authenticationClients = new AuthenticationClientCollection();

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated by an OAuth provider.
        /// </summary>
        public static bool IsAuthenticatedWithOAuth
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
                }

                return GetIsAuthenticatedWithOAuthCore(new HttpContextWrapper(HttpContext.Current));
            }
        }

        /// <summary>
        /// Registers a supported OAuth client with the specified consumer key and consumer secret.
        /// </summary>
        /// <param name="client">One of the supported OAuth clients.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        public static void RegisterOAuthClient(BuiltInOAuthClient client, string consumerKey, string consumerSecret)
        {
            IAuthenticationClient authenticationClient;
            switch (client)
            {
                case BuiltInOAuthClient.LinkedIn:
                    authenticationClient = new LinkedInClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.Twitter:
                    authenticationClient = new TwitterClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.Facebook:
                    authenticationClient = new FacebookClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.WindowsLive:
                    authenticationClient = new WindowsLiveClient(consumerKey, consumerSecret);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("client");
            }
            RegisterClient(authenticationClient);
        }

        /// <summary>
        /// Registers a supported OpenID client
        /// </summary>
        public static void RegisterOpenIDClient(BuiltInOpenIDClient openIDClient)
        {
            IAuthenticationClient client;
            switch (openIDClient)
            {
                case BuiltInOpenIDClient.Google:
                    client = new GoogleOpenIdClient();
                    break;

                case BuiltInOpenIDClient.Yahoo:
                    client = new YahooOpenIdClient();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("openIDClient");
            }

            RegisterClient(client);
        }

        /// <summary>
        /// Registers an authentication client.
        /// </summary>
        [CLSCompliant(false)]
        public static void RegisterClient(IAuthenticationClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (String.IsNullOrEmpty(client.ProviderName))
            {
                throw new ArgumentException(WebResources.InvalidServiceProviderName, "client");
            }

            if (_authenticationClients.Contains(client))
            {
                throw new ArgumentException(WebResources.ServiceProviderNameExists, "client");
            }

            _authenticationClients.Add(client);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        public static void RequestAuthentication(string provider)
        {
            RequestAuthentication(provider, returnUrl: null);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return url after user is authenticated.</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "We want to allow relative app path, and support ~/")]
        public static void RequestAuthentication(string provider, string returnUrl)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            RequestAuthenticationCore(new HttpContextWrapper(HttpContext.Current), provider, returnUrl);
        }

        internal static void RequestAuthenticationCore(HttpContextBase context, string provider, string returnUrl)
        {
            IAuthenticationClient client = GetOAuthClient(provider);
            var securityManager = new OpenAuthSecurityManager(context, client, OAuthDataProvider);
            securityManager.RequestAuthentication(returnUrl);
        }

        /// <summary>
        /// Checks if user is successfully authenticated when user is redirected back to this user.
        /// </summary>
        [CLSCompliant(false)]
        public static AuthenticationResult VerifyAuthentication()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return VerifyAuthenticationCore(new HttpContextWrapper(HttpContext.Current));
        }

        internal static AuthenticationResult VerifyAuthenticationCore(HttpContextBase context)
        {
            string providerName = OpenAuthSecurityManager.GetProviderName(context);
            if (String.IsNullOrEmpty(providerName))
            {
                return AuthenticationResult.Failed;
            }

            IAuthenticationClient client;
            if (TryGetOAuthClient(providerName, out client))
            {
                var securityManager = new OpenAuthSecurityManager(context, client, OAuthDataProvider);
                return securityManager.VerifyAuthentication();
            }
            else
            {
                throw new InvalidOperationException(WebResources.InvalidServiceProviderName);
            }
        }

        /// <summary>
        /// Checks if the specified provider user id represents a valid account.
        /// If it does, log user in.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="createPersistentCookie">if set to <c>true</c> create persistent cookie as part of the login.</param>
        /// <returns>
        ///   <c>true</c> if the login is successful.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
        public static bool Login(string providerName, string providerUserId, bool createPersistentCookie)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return LoginCore(new HttpContextWrapper(HttpContext.Current), providerName, providerUserId, createPersistentCookie);
        }

        internal static bool LoginCore(HttpContextBase context, string providerName, string providerUserId, bool createPersistentCookie)
        {
            var provider = GetOAuthClient(providerName);
            var securityManager = new OpenAuthSecurityManager(context, provider, OAuthDataProvider);
            return securityManager.Login(providerUserId, createPersistentCookie);
        }

        internal static bool GetIsAuthenticatedWithOAuthCore(HttpContextBase context)
        {
            return new OpenAuthSecurityManager(context).IsAuthenticatedWithOpenAuth;
        }

        /// <summary>
        /// Creates or update the account with the specified provider, provider user id and associate it with the specified user name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="userName">The user name.</param>
        public static void CreateOrUpdateAccount(string providerName, string providerUserId, string userName)
        {
            ExtendedMembershipProvider provider = VerifyProvider();
            provider.CreateOrUpdateOAuthAccount(providerName, providerUserId, userName);
        }

        /// <summary>
        /// Gets the registered user name corresponding to the specified provider and provider user id.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <returns></returns>
        public static string GetUserName(string providerName, string providerUserId)
        {
            return OAuthDataProvider.GetUserNameFromOpenAuth(providerName, providerUserId);
        }

        /// <summary>
        /// Gets all OAuth &amp; OpenID accounts which are associted with the specified user name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public static ICollection<OAuthAccount> GetAccountsFromUserName(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "userName"),
                    "userName");
            }

            ExtendedMembershipProvider provider = VerifyProvider();
            return provider.GetAccountsForUser(userName).Select(p => new OAuthAccount(p.Provider, p.ProviderUserId)).ToList();
        }

        /// <summary>
        /// Delete the specified OAuth &amp; OpenID account
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public static bool DeleteAccount(string providerName, string providerUserId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            string username = GetUserName(providerName, providerUserId);
            if (String.IsNullOrEmpty(username)) 
            {
                // account doesn't exist
                return false;
            }

            provider.DeleteOAuthAccount(providerName, providerName);
            return true;
        }

        internal static IAuthenticationClient GetOAuthClient(string providerName)
        {
            if (!_authenticationClients.Contains(providerName))
            {
                throw new ArgumentException(WebResources.ServiceProviderNotFound, "providerName");
            }

            return _authenticationClients[providerName];
        }

        internal static bool TryGetOAuthClient(string provider, out IAuthenticationClient client)
        {
            if (_authenticationClients.Contains(provider))
            {
                client = _authenticationClients[provider];
                return true;
            }
            else
            {
                client = null;
                return false;
            }
        }

        /// <summary>
        /// for unit tests
        /// </summary>
        internal static void ClearProviders()
        {
            _authenticationClients.Clear();
        }

        private static ExtendedMembershipProvider VerifyProvider() 
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null) 
            {
                throw new InvalidOperationException();
            }
            return provider;
        }
    }
}