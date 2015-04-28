using System;
using System.IO;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// Read and write content from disk file stream.
    /// </summary>
    public class DiskFileStreamProvider : IFileStreamProvider
    {
        private FileInfo _file;

        public DiskFileStreamProvider(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            file.Refresh();
            if (!file.Exists)
            {
                file.Create();
            }

            _file = file;
        }

        public Stream OpenRead()
        {
            return _file.OpenRead();
        }

        public Stream OpenWrite()
        {
            return _file.OpenWrite();
        }
    }
}
