using System.IO;
using WebStack.QA.Common.Extensions;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// This provider is used to provide temporary directory for testing.
    /// It accepts different nameStrategy, like GUID, prefix increasing or fixed name.
    /// By default, it uses GUID.
    /// </summary>
    public class TemporaryDirectoryProvider : ITemporaryDirectoryProvider
    {
        private string _root;
        private IDirectoryNameStrategy _nameStrategy;

        public TemporaryDirectoryProvider(IDirectoryNameStrategy nameStrategy)
        {
            _root = Path.GetTempPath();
            _nameStrategy = nameStrategy;
        }

        public DirectoryInfo CreateDirectory()
        {
            var path = Path.Combine(_root, _nameStrategy.GetName());

            var dir = new DirectoryInfo(path);
            dir.Recreate();

            return dir;
        }
    }
}
