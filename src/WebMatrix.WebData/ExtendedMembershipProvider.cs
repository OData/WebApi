// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Security;

namespace WebMatrix.WebData
{
    public abstract class ExtendedMembershipProvider : MembershipProvider
    {
        private const int OneDayInMinutes = 24 * 60;

        /// <summary>
        /// Deletes the OAuth and OpenID account with the specified provider name and provider user id.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public abstract void DeleteOAuthAccount(string provider, string providerUserId);

        /// <summary>
        /// Creates a new OAuth account with the specified data or update an existing one if it already exists.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider userid.</param>
        /// <param name="userName">The username.</param>
        public abstract void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string userName);

        /// <summary>
        /// Gets the id of the user with the specified provider name and provider user id.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <returns></returns>
        public abstract int GetUserIdFromOAuth(string provider, string providerUserId);

        /// <summary>
        /// Gets the username of a user with the given id
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public abstract string GetUserNameFromId(int userId);

        /// <summary>
        /// Gets all OAuth accounts associated with the specified username
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public abstract ICollection<OAuthAccountData> GetAccountsForUser(string userName);

        public virtual string CreateUserAndAccount(string userName, string password)
        {
            return CreateUserAndAccount(userName, password, requireConfirmation: false, values: null);
        }

        public virtual string CreateUserAndAccount(string userName, string password, bool requireConfirmation)
        {
            return CreateUserAndAccount(userName, password, requireConfirmation, values: null);
        }

        public virtual string CreateUserAndAccount(string userName, string password, IDictionary<string, object> values)
        {
            return CreateUserAndAccount(userName, password, requireConfirmation: false, values: values);
        }

        public abstract string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values);

        public virtual string CreateAccount(string userName, string password)
        {
            return CreateAccount(userName, password, requireConfirmationToken: false);
        }

        public abstract string CreateAccount(string userName, string password, bool requireConfirmationToken);
        public abstract bool ConfirmAccount(string userName, string accountConfirmationToken);
        public abstract bool ConfirmAccount(string accountConfirmationToken);
        public abstract bool DeleteAccount(string userName);

        public virtual string GeneratePasswordResetToken(string userName)
        {
            return GeneratePasswordResetToken(userName, tokenExpirationInMinutesFromNow: OneDayInMinutes);
        }

        public abstract string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow);
        public abstract int GetUserIdFromPasswordResetToken(string token);
        public abstract bool IsConfirmed(string userName);
        public abstract bool ResetPasswordWithToken(string token, string newPassword);
        public abstract int GetPasswordFailuresSinceLastSuccess(string userName);
        public abstract DateTime GetCreateDate(string userName);
        public abstract DateTime GetPasswordChangedDate(string userName);
        public abstract DateTime GetLastPasswordFailureDate(string userName);

        internal virtual void VerifyInitialized()
        {
        }
    }
}
