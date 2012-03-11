using System.IO;

namespace System.Web.WebPages.Deployment
{
    internal interface IBuildManager
    {
        Stream CreateCachedFile(string fileName);

        Stream ReadCachedFile(string fileName);
    }
}
