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
    }
}
