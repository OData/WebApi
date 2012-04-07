// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Internal.Web.Utils
{
    internal sealed class PhysicalFileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream ReadFile(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenFile(string path)
        {
            string directory = Path.GetDirectoryName(path);
            EnsureDirectory(directory);
            return File.OpenWrite(path);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
