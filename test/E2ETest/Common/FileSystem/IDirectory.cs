using System.Collections.Generic;

namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// The interface is designed to abstract deployment source, which can 
    /// come from file system, in memory data, or resource file from assembly.
    /// </summary>
    public interface IDirectory
    {
        string Name { get; set; }
        IDirectory Parent { get; set; }
        string FullName { get; }

        IEnumerable<IFile> GetSubFiles();
        IEnumerable<IDirectory> GetSubDirectories();

        IDirectory CreateDirectory(string name);
        IFile CreateFile(IFile file);

        void RemoveDirectory(IDirectory directory);
        void RemoveFile(IFile file);

        bool DirectoryExists(string name);
        bool FileExists(string name);
    }
}
