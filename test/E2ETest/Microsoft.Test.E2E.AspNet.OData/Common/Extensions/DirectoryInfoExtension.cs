using System.IO;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
    public static class DirectoryInfoExtension
    {
        /// <summary>
        /// Delete and create the folder if it already exists
        /// </summary>
        public static void Recreate(this DirectoryInfo self)
        {
            self.Refresh();
            if (self.Exists)
            {
                self.Delete(true);
            }
            self.Create();
        }
    }
}