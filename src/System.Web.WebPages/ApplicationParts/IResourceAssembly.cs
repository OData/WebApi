using System.Collections.Generic;
using System.IO;

namespace System.Web.WebPages.ApplicationParts
{
    // For unit testing purpose since Assembly is not Moqable
    internal interface IResourceAssembly
    {
        string Name { get; }
        Stream GetManifestResourceStream(string name);
        IEnumerable<string> GetManifestResourceNames();
        IEnumerable<Type> GetTypes();
    }
}
