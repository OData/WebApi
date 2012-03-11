namespace System.Web.WebPages
{
    internal sealed class WebPageMatch
    {
        public WebPageMatch(string matchedPath, string pathInfo)
        {
            MatchedPath = matchedPath;
            PathInfo = pathInfo;
        }

        public string MatchedPath { get; private set; }

        public string PathInfo { get; private set; }
    }
}
