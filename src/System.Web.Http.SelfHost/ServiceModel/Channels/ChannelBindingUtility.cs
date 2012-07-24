// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Authentication.ExtendedProtection;
using System.Security.Authentication.ExtendedProtection.Configuration;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal static class ChannelBindingUtility
    {
        private static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
        private static ExtendedProtectionPolicy defaultPolicy = disabledPolicy;

        public static bool IsDefaultPolicy(ExtendedProtectionPolicy policy)
        {
            return Object.ReferenceEquals(policy, defaultPolicy);
        }
    }
}
