using System.IO;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// Represents the file content stream provider
    /// Note that the stream should be closed by caller
    /// </summary>
    public interface IFileStreamProvider
    {
        Stream OpenRead();
        Stream OpenWrite();
    }
}
