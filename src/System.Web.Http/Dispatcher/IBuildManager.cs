using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Provides an abstraction for managing the compilation of an application. A different
    /// implementation can be registered via the <see cref="T:System.Web.Http.Servies.DependencyResolver"/>.
    /// </summary>
    public interface IBuildManager
    {
        /// <summary>
        /// Gets an object factory for the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns><c>true</c> if file exists; otherwise false.</returns>
        bool FileExists(string virtualPath);

        /// <summary>
        /// Compiles a file, given its virtual path, and returns the compiled type.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The compiled <see cref="Type"/>.</returns>
        Type GetCompiledType(string virtualPath);

        /// <summary>
        /// Returns a list of assembly references that all page compilations must reference.
        /// </summary>
        /// <returns>An <see cref="ICollection"/> of assembly references.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is better handled as a method.")]
        ICollection GetReferencedAssemblies();

        /// <summary>
        /// Reads a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the file, or <c>null</c> if the file does not exist.</returns>
        Stream ReadCachedFile(string fileName);

        /// <summary>
        /// Creates a cached file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The <see cref="Stream"/> object for the new file.</returns>
        Stream CreateCachedFile(string fileName);
    }
}
