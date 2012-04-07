// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using WebMatrix.WebData;

namespace Microsoft.Web.WebPages.OAuth
{
    internal class WebPagesOAuthDataProvider : IOpenAuthDataProvider
    {
        private static ExtendedMembershipProvider VerifyProvider()
        {
            var provider = Membership.Provider as ExtendedMembershipProvider;
            if (provider == null)
            {
                throw new InvalidOperationException();
            }
            return provider;
        }

        public string GetUserNameFromOpenAuth(string openAuthProvider, string openAuthId)
        {
            ExtendedMembershipProvider provider = VerifyProvider();

            int userId = provider.GetUserIdFromOAuth(openAuthProvider, openAuthId);
            if (userId == -1) 
            {
                return null;
            }

            return provider.GetUserNameFromId(userId);
        }
    }
}