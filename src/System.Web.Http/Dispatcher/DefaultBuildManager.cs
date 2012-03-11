using System.Collections;
using System.IO;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an implementation of <see cref="IBuildManager"/> with no external dependencies.
    /// </summary>
    internal class DefaultBuildManager : IBuildManager
    {
        /// <summary>
        /// Gets an object factory for the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns><c>true</c> if file exists; otherwise false.</returns>
        bool IBuildManager.FileExists(string virtualPath)
        {
            return false;
        }

        /// <summary>
        /// Compiles a file, given its virtual path, and returns the compiled type.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The compiled <see cref="Type"/>.</returns>
        Type IBuildManager.GetCompiledType(string virtualPath)
        {
            return null;
        }

        /// <summary>
        /// Returns a list of assembly references that all page compilations must reference.
        /// </summary>
        /// <returns>An <see cref="ICollection"/> of assembly references.</returns>
        ICollection IBuildManager.GetReferencedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Reads a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the file, or <c>null</c> if the file does not exist.</returns>
        Stream IBuildManager.ReadCachedFile(string fileName)
        {
            return null;
        }

        /// <summary>
        /// Creates a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the new file.</returns>
        Stream IBuildManager.CreateCachedFile(string fileName)
        {
            return Stream.Null;
        }
    }
}
