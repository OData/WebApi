// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace WebMatrix.WebData.Test
{
    public class WebSecurityTest
    {
        [Fact]
        public void VerifyExtendedMembershipProviderMethodsThrowWithInvalidProvider()
        {
            const string errorString = "To call this method, the \"Membership.Provider\" property must be an instance of \"ExtendedMembershipProvider\".";
            Assert.Throws<InvalidOperationException>(() => WebSecurity.ConfirmAccount(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GeneratePasswordResetToken(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetUserIdFromPasswordResetToken(""), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.ResetPassword("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.CreateUserAndAccount("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.CreateAccount("", "whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.IsConfirmed("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetPasswordFailuresSinceLastSuccess("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetCreateDate("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetLastPasswordFailureDate("whatever"), errorString);
            Assert.Throws<InvalidOperationException>(() => WebSecurity.GetPasswordChangedDate("whatever"), errorString);
        }
    }
}
