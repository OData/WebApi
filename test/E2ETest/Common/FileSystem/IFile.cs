
namespace WebStack.QA.Common.FileSystem
{
    /// <summary>
    /// Abstraction file which represents deployment source
    /// </summary>
    public interface IFile
    {
        string Name { get; set; }
        string Extension { get; set; }
        IDirectory Directory { get; set; }

        IFileStreamProvider StreamProvider { get; set; }
    }
}
