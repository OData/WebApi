using System;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Azure website host doesn't nothing as azure website should keep running state. 
    /// The host just output the site url to external caller.
    /// </summary>
    public class AzureWebsiteHostOptions : HostOptions
    {
        public AzureWebsiteHostOptions(string siteRootUrl)
        {
            if (string.IsNullOrEmpty(siteRootUrl))
            {
                throw new ArgumentNullException("siteRootUrl");
            }

            SiteRootUrl = siteRootUrl;
        }

        public string SiteRootUrl { get; set; }

        public override string Start()
        {
            return SiteRootUrl;
        }

        public override void Stop()
        {
        }
    }
}
