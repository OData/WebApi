using System.Collections;
using System.IO;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class DefaultBuildManagerTest
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<DefaultBuildManager, IBuildManager>(TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        public void Constructor()
        {
            Assert.NotNull(new DefaultBuildManager());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("path")]
        public void FileExists(string path)
        {
            IBuildManager buildManager = new DefaultBuildManager();
            Assert.False(buildManager.FileExists(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("path")]
        public void GetCompiledType(string path)
        {
            IBuildManager buildManager = new DefaultBuildManager();
            Assert.Null(buildManager.GetCompiledType(path));
        }

        [Fact]
        public void GetReferencedAssemblies()
        {
            IBuildManager buildManager = new DefaultBuildManager();
            ICollection assemblies = buildManager.GetReferencedAssemblies();
            Assert.NotEmpty(assemblies);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("path")]
        public void ReadCachedFile(string path)
        {
            IBuildManager buildManager = new DefaultBuildManager();
            Assert.Null(buildManager.ReadCachedFile(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("path")]
        public void CreateCachedFile(string path)
        {
            IBuildManager buildManager = new DefaultBuildManager();
            Assert.Same(Stream.Null, buildManager.CreateCachedFile(path));
        }
    }
}
