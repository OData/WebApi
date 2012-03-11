using System.Collections.Generic;

namespace System.Web.WebPages.Administration.PackageManager
{
    public interface IPackagesSourceFile
    {
        bool Exists();

        void WriteSources(IEnumerable<WebPackageSource> sources);

        IEnumerable<WebPackageSource> ReadSources();
    }
}
