using System;

namespace WebStack.QA.Common.WebHost.Xunit
{
    public class WebHostSetupFixture<TSetupOptionsProvider> :
        IDisposable
        where TSetupOptionsProvider : IWebAppSetupOptionsProvider, new()
    {
        #region Constants and Fields

        private readonly WebAppSetupOptions options;

        #endregion

        #region Constructors and Destructors

        public WebHostSetupFixture()
        {
            this.options = new TSetupOptionsProvider().GetSetupOptions();
            this.VDirAddress = IISHelper.SetupIIS(this.options);
        }

        #endregion

        #region Public Properties

        public string VDirAddress { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IISHelper.CleanupIIS(this.options);
            }
        }

        #endregion
    }
}