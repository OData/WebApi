// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Security.Authentication.ExtendedProtection;
using System.Security.Authentication.ExtendedProtection.Configuration;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal static class ChannelBindingUtility
    {
        private static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
        private static ExtendedProtectionPolicy defaultPolicy = disabledPolicy;

        public static ExtendedProtectionPolicy DisabledPolicy
        {
            get { return disabledPolicy; }
        }

        public static ExtendedProtectionPolicy DefaultPolicy
        {
            get { return defaultPolicy; }
        }

        public static bool IsDefaultPolicy(ExtendedProtectionPolicy policy)
        {
            return Object.ReferenceEquals(policy, defaultPolicy);
        }

        public static void InitializeFrom(ExtendedProtectionPolicy source, ExtendedProtectionPolicyElement destination)
        {
            if (!IsDefaultPolicy(source))
            {
                destination.PolicyEnforcement = source.PolicyEnforcement;
                destination.ProtectionScenario = source.ProtectionScenario;
                destination.CustomServiceNames.Clear();

                if (source.CustomServiceNames != null)
                {
                    foreach (string name in source.CustomServiceNames)
                    {
                        ServiceNameElement entry = new ServiceNameElement();
                        entry.Name = name;
                        destination.CustomServiceNames.Add(entry);
                    }
                }
            }
        }

        public static ExtendedProtectionPolicy BuildPolicy(ExtendedProtectionPolicyElement configurationPolicy)
        {
            // using this pattern allows us to have a different default policy
            // than the NCL team chooses.
            if (configurationPolicy.ElementInformation.IsPresent)
            {
                return configurationPolicy.BuildPolicy();
            }
            else
            {
                return ChannelBindingUtility.DefaultPolicy;
            }
        }
    }
}
