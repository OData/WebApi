// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Web.Mvc.Test
{
    // Custom mock IBuildManager since the mock framework doesn't support mocking internal types
    public class MockBuildManager : IBuildManager
    {
        private Assembly[] _referencedAssemblies;

        private Type _compiledType;
        private string _expectedVirtualPath;
        private bool _fileExists = true;

        public readonly Dictionary<string, Stream> CachedFileStore = new Dictionary<string, Stream>(StringComparer.OrdinalIgnoreCase);

        public MockBuildManager()
            : this(new Assembly[] { typeof(MockBuildManager).Assembly })
        {
        }

        public MockBuildManager(Assembly[] referencedAssemblies)
        {
            _referencedAssemblies = referencedAssemblies;
        }

        public MockBuildManager(string expectedVirtualPath, bool fileExists)
        {
            _expectedVirtualPath = expectedVirtualPath;
            _fileExists = fileExists;
        }

        public MockBuildManager(string expectedVirtualPath, Type compiledType)
        {
            _expectedVirtualPath = expectedVirtualPath;
            _compiledType = compiledType;
        }

        bool IBuildManager.FileExists(string virtualPath)
        {
            if (_expectedVirtualPath == virtualPath)
            {
                return _fileExists;
            }

            throw new InvalidOperationException("Unexpected call to IBuildManager.FileExists()");
        }

        public Type GetCompiledType(string virtualPath)
        {
            if (_expectedVirtualPath == virtualPath)
            {
                return _compiledType;
            }

            throw new InvalidOperationException("Unexpected call to IBuildManager.GetCompiledType()");
        }

        ICollection IBuildManager.GetReferencedAssemblies()
        {
            return _referencedAssemblies;
        }

        Stream IBuildManager.ReadCachedFile(string fileName)
        {
            Stream stream;
            CachedFileStore.TryGetValue(fileName, out stream);
            return stream;
        }

        Stream IBuildManager.CreateCachedFile(string fileName)
        {
            MemoryStream stream = new MemoryStream();
            CachedFileStore[fileName] = stream;
            return stream;
        }
    }
}
