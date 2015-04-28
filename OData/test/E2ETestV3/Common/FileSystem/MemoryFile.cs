using System;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// In memory file which contains file's properties and content
    /// </summary>
    public class MemoryFile : IFile
    {
        public MemoryFile(string name, IDirectory directory, IFileStreamProvider streamProvider = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Directory = directory;
            Extension = ParseExtension(name);
            if (streamProvider == null)
            {
                streamProvider = new MemoryStreamProvider();
            }
            StreamProvider = streamProvider;
        }

        public string Name
        {
            get;
            set;
        }

        public string Extension
        {
            get;
            set;
        }

        public IFileStreamProvider StreamProvider
        {
            get;
            set;
        }

        public IDirectory Directory
        {
            get;
            set;
        }

        private string ParseExtension(string name)
        {
            var index = name.LastIndexOf('.');
            if (index > 0)
            {
                return name.Substring(index);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
