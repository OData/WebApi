// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Web.Hosting;
using Moq;
using Xunit;

namespace System.Web.WebPages.Test
{
    public class FileExistenceCacheTest
    {
        [Fact]
        public void ConstructorTest()
        {
            var ms = 1000;
            var cache = new FileExistenceCache(null);
            Assert.Null(cache.VirtualPathProvider);

            var vpp = new Mock<VirtualPathProvider>().Object;
            cache = new FileExistenceCache(vpp);
            Assert.Equal(vpp, cache.VirtualPathProvider);
            Assert.Equal(ms, cache.MilliSecondsBeforeReset);

            ms = 9999;
            cache = new FileExistenceCache(vpp, ms);
            Assert.Equal(vpp, cache.VirtualPathProvider);
            Assert.Equal(ms, cache.MilliSecondsBeforeReset);
        }

        [Fact]
        public void TimeExceededFalseTest()
        {
            var ms = 100000;
            var cache = new FileExistenceCache(GetVpp(), ms);
            Assert.False(cache.TimeExceeded);
        }

        [Fact]
        public void TimeExceededTrueTest()
        {
            var ms = 5;
            var cache = new FileExistenceCache(GetVpp(), ms);
            Thread.Sleep(300);
            Assert.True(cache.TimeExceeded);
        }

        [Fact]
        public void ResetTest()
        {
            var cache = new FileExistenceCache(GetVpp());
            var cacheInternal = cache.CacheInternal;
            cache.Reset();
            Assert.NotSame(cacheInternal, cache.CacheInternal);
        }

        [Fact]
        public void FileExistsTest()
        {
            var path = "~/index.cshtml";
            var cache = new FileExistenceCache(GetVpp(path));
            Assert.True(cache.FileExists(path));
            Assert.False(cache.FileExists("~/test.cshtml"));
        }

        [Fact]
        public void FileExistsVppLaterTest()
        {
            var path = "~/index.cshtml";
            var cache = new FileExistenceCache(GetVpp(path));
            Assert.True(cache.FileExists(path));
            Assert.False(cache.FileExists("~/test.cshtml"));
        }

        [Fact]
        public void FileExistsTimeExceededTest()
        {
            var path = "~/index.cshtml";
            Utils.SetupVirtualPathInAppDomain(path, "");

            var cache = new FileExistenceCache(GetVpp(path));
            var cacheInternal = cache.CacheInternal;
            cache.MilliSecondsBeforeReset = 5;
            Thread.Sleep(300);
            Assert.True(cache.FileExists(path));
            Assert.False(cache.FileExists("~/test.cshtml"));
            Assert.NotEqual(cacheInternal, cache.CacheInternal);
        }

        private static VirtualPathProvider GetVpp(params string[] files)
        {
            var vpp = new Mock<VirtualPathProvider>();
            vpp.Setup(c => c.FileExists(It.IsAny<string>())).Returns<string>(p => files.Contains(p, StringComparer.OrdinalIgnoreCase));
            return vpp.Object;
        }
    }
}
