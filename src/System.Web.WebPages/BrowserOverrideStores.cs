namespace System.Web.WebPages
{
    public class BrowserOverrideStores
    {
        private static BrowserOverrideStores _instance = new BrowserOverrideStores();
        private BrowserOverrideStore _currentOverrideStore = new CookieBrowserOverrideStore();

        /// <summary>
        /// The current BrowserOverrideStore
        /// </summary>
        public static BrowserOverrideStore Current
        {
            get { return _instance.CurrentInternal; }
            set { _instance.CurrentInternal = value; }
        }

        internal BrowserOverrideStore CurrentInternal
        {
            get { return _currentOverrideStore; }
            set { _currentOverrideStore = value ?? new RequestBrowserOverrideStore(); }
        }
    }
}
