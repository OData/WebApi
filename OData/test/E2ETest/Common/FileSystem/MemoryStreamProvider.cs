using System.IO;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// Read and write content from memory stream.
    /// </summary>
    public class MemoryStreamProvider : IFileStreamProvider
    {
        private Stream _content = new MemoryStream();

        public Stream OpenRead()
        {
            _content.Position = 0;
            return new StreamWrapper(_content);
        }

        public Stream OpenWrite()
        {
            if (_content != null)
            {
                _content.Dispose();
            }
            _content = new MemoryStream();
            return new StreamWrapper(_content);
        }
    }
}
