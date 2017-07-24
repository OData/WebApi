using System.IO;

namespace WebStack.QA.Common.WebHost
{
    public interface ITemporaryDirectoryProvider
    {
        /// <summary>
        /// Create a new directory. The implementation must guarantee the new created directory is empty
        /// </summary>
        /// <returns></returns>
        DirectoryInfo CreateDirectory();
    }
}
