// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Helpers.AntiXsrf.Test
{
    public sealed class MockAntiForgeryConfig : IAntiForgeryConfig
    {
        public IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get;
            set;
        }

        public string CookieName
        {
            get;
            set;
        }

        public string FormFieldName
        {
            get;
            set;
        }

        public bool RequireSSL
        {
            get;
            set;
        }

        public bool SuppressIdentityHeuristicChecks
        {
            get;
            set;
        }

        public string UniqueClaimTypeIdentifier
        {
            get;
            set;
        }

        public bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }
    }
}
