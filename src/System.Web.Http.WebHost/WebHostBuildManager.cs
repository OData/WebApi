using System.Collections;
using System.IO;
using System.Web.Compilation;
using System.Web.Http.Dispatcher;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Wraps ASP build manager
    /// </summary>
    internal sealed class WebHostBuildManager : IBuildManager
    {
        /// <summary>
        /// Gets an object factory for the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns><c>true</c> if file exists; otherwise false.</returns>
        bool IBuildManager.FileExists(string virtualPath)
        {
            return BuildManager.GetObjectFactory(virtualPath, false) != null;
        }

        /// <summary>
        /// Compiles a file, given its virtual path, and returns the compiled type.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The compiled <see cref="Type"/>.</returns>
        Type IBuildManager.GetCompiledType(string virtualPath)
        {
            return BuildManager.GetCompiledType(virtualPath);
        }

        /// <summary>
        /// Returns a list of assembly references that all page compilations must reference.
        /// </summary>
        /// <returns>An <see cref="ICollection"/> of assembly references.</returns>
        ICollection IBuildManager.GetReferencedAssemblies()
        {
            return BuildManager.GetReferencedAssemblies();
        }

        /// <summary>
        /// Reads a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the file, or <c>null</c> if the file does not exist.</returns>
        Stream IBuildManager.ReadCachedFile(string fileName)
        {
            return BuildManager.ReadCachedFile(fileName);
        }

        /// <summary>
        /// Creates a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the new file.</returns>
        Stream IBuildManager.CreateCachedFile(string fileName)
        {
            return BuildManager.CreateCachedFile(fileName);
        }
    }
}
