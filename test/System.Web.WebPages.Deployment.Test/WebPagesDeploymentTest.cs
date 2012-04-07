// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.WebPages.TestUtils;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.WebPages.Deployment.Test
{
    // We need to mark this type Serializable for TDD.Net to work with some of the AppDomain tests
    [Serializable]
    public class WebPagesDeploymentTest : IDisposable
    {
        private const string TestNamespacePrefix = "System.Web.WebPages.Deployment.Test.TestFiles.";
        private static readonly Version MaxVersion = new Version(2, 0, 0, 0);

        private static readonly IDictionary<string, string> _deploymentPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { @"ConfigTestSites.CshtmlFileNoVersion.Default.cshtml", @"ConfigTestSites\CshtmlFileNoVersion\Default.cshtml" },
            { @"ConfigTestSites.NoCshtmlWithEnabledSetting.Default.htm", @"ConfigTestSites\NoCshtmlWithEnabledSetting\Default.htm" },
            { @"ConfigTestSites.NoCshtmlNoConfigSetting.web.config", @"ConfigTestSites\NoCshtmlNoConfigSetting\web.config" },
            { @"ConfigTestSites.NoCshtmlWithEnabledSetting.web.config", @"ConfigTestSites\NoCshtmlWithEnabledSetting\web.config" },
            { @"ConfigTestSites.NoCshtmlWithEnabledSettingFalse.Default.htm", @"ConfigTestSites\NoCshtmlWithEnabledSettingFalse\Default.htm" },
            { @"ConfigTestAssemblies.V2_Unsigned.System.Web.WebPages.Deployment.dll", @"ConfigTestAssemblies\V2_Unsigned\System.Web.WebPages.Deployment.dll" },
            { @"ConfigTestAssemblies.V2_Signed.System.Web.WebPages.Deployment.dll", @"ConfigTestAssemblies\V2_Signed\System.Web.WebPages.Deployment.dll" },
            { @"ConfigTestSites.NoCshtmlWithEnabledSettingFalse.web.config", @"ConfigTestSites\NoCshtmlWithEnabledSettingFalse\web.config" },
            { @"ConfigTestSites.CshtmlFileConfigV1.Default.cshtml", @"ConfigTestSites\CshtmlFileConfigV1\Default.cshtml" },
            { @"ConfigTestSites.NoCshtml.Default.htm", @"ConfigTestSites\NoCshtml\Default.htm" },
            { @"ConfigTestSites.NoCshtmlNoConfigSetting.Default.htm", @"ConfigTestSites\NoCshtmlNoConfigSetting\Default.htm" },
            { @"ConfigTestSites.CshtmlFileConfigV1.web.config", @"ConfigTestSites\CshtmlFileConfigV1\web.config" },
            { @"ConfigTestSites.NoCshtmlConfigV1.Default.htm", @"ConfigTestSites\NoCshtmlConfigV1\Default.htm" },
            { @"ConfigTestSites.NoCshtmlConfigV1.web.config", @"ConfigTestSites\NoCshtmlConfigV1\web.config" },
        };

        private readonly string _tempPath = GetTempPath();

        public WebPagesDeploymentTest()
        {
            var assembly = typeof(WebPagesDeploymentTest).Assembly;
            foreach (var item in _deploymentPaths)
            {
                new TestFile(TestNamespacePrefix + item.Key, assembly).Save(Path.Combine(_tempPath, item.Value));
            }
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_tempPath, recursive: true);
            }
            catch
            {

            }
        }

        [Fact]
        public void IsEnabledReturnsFalseIfNoCshtmlOrConfigFile()
        {
            Assert.False(WebPagesDeployment.IsEnabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtml")));
        }

        [Fact]
        public void IsEnabledReturnsFalseIfNoCshtmlAndNoConfigSetting()
        {
            Assert.False(WebPagesDeployment.IsEnabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlNoConfigSetting")));
        }

        [Fact]
        public void IsEnabledReturnsTrueIfNoCshtmlAndEnabledConfigSetting()
        {
            Assert.True(WebPagesDeployment.IsEnabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlWithEnabledSetting")));
        }

        [Fact]
        public void IsEnabledReturnsTrueIfCshtmlFilePresent()
        {
            Assert.True(WebPagesDeployment.IsEnabled(Path.Combine(_tempPath, @"ConfigTestSites\CshtmlFileNoVersion")));
        }

        [Fact]
        public void IsExplicitlyDisabledReturnsTrueIfNoCshtmlAndEnabledConfigSettingSetToFalse()
        {
            Assert.True(WebPagesDeployment.IsExplicitlyDisabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlWithEnabledSettingFalse")));
        }

        [Fact]
        public void IsExplicitlyDisabledReturnsFalseIfNoCshtmlAndEnabledConfigSettingSetToTrue()
        {
            Assert.False(WebPagesDeployment.IsExplicitlyDisabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlWithEnabledSetting")));
        }

        [Fact]
        public void IsExplicitlyDisabledReturnsFalseIfNoCshtmlAndNoConfigSetting()
        {
            Assert.False(WebPagesDeployment.IsExplicitlyDisabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlNoConfigSetting")));
        }

        [Fact]
        public void IsExplicitlyDisabledReturnsFalseIfNoCshtmlOrConfigFile()
        {
            Assert.False(WebPagesDeployment.IsExplicitlyDisabled(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtml")));
        }

        [Fact]
        public void GetVersionReturnsValueFromAppSettingsIfNotExplicitlyDisabled()
        {
            // Arrange
            var version = "1.2.3.4";
            var appSettings = new NameValueCollection { { "webPages:Version", version } };
            var maxVersion = new Version("2.0");

            // Act
            var actualVersion = WebPagesDeployment.GetVersionInternal(appSettings, binVersion: null, defaultVersion: null);

            // Assert
            Assert.Equal(new Version(version), actualVersion);
        }

        [Fact]
        public void GetVersionReturnsValueEvenIfExplicitlyDisabled()
        {
            // Arrange
            var version = "1.2.3.4";
            var appSettings = new NameValueCollection { { "webPages:Version", version }, { "webPages:Enabled", "False" } };
            var maxVersion = new Version("2.0");

            // Act
            var actualVersion = WebPagesDeployment.GetVersionInternal(appSettings, binVersion: null, defaultVersion: null);

            // Assert
            Assert.Equal(new Version(version), actualVersion);
        }

        [Fact]
        public void GetVersionReturnsLowerVersionIfSpecifiedInConfig()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange - Load v2 Config
                Assembly asm = Assembly.LoadFrom(Path.Combine(_tempPath, @"ConfigTestAssemblies\V2_Signed\System.Web.WebPages.Deployment.dll"));
                Assert.Equal(new Version(2, 0, 0, 0), asm.GetName().Version);
                Assert.Equal("System.Web.WebPages.Deployment", asm.GetName().Name);

                using (WebUtils.CreateHttpRuntime(@"~\foo", "."))
                {
                    // Act
                    Version ver = WebPagesDeployment.GetVersionWithoutEnabledCheck(Path.Combine(_tempPath, @"ConfigTestSites\CshtmlFileConfigV1"));

                    // Assert
                    Assert.Equal(new Version(1, 0, 0, 0), ver);
                }
            });
        }

        [Fact]
        public void GetVersionReturnsLowerVersionIfSpecifiedInConfigAndNotExplicitlyDisabled()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange - Load v2 Config
                Assembly asm = Assembly.LoadFrom(Path.Combine(_tempPath, @"ConfigTestAssemblies\V2_Signed\System.Web.WebPages.Deployment.dll"));
                Assert.Equal(new Version(2, 0, 0, 0), asm.GetName().Version);
                Assert.Equal("System.Web.WebPages.Deployment", asm.GetName().Name);

                using (WebUtils.CreateHttpRuntime(@"~\foo", "."))
                {
                    // Act
                    Version ver = WebPagesDeployment.GetVersionWithoutEnabledCheck(Path.Combine(_tempPath, @"ConfigTestSites\NoCshtmlConfigV1"));

                    // Assert
                    Assert.Equal(new Version(1, 0, 0, 0), ver);
                }
            });
        }

        [Fact]
        public void GetVersionIgnoresUnsignedConfigDll()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange - Load v2 Config
                Assembly asm = Assembly.LoadFrom(Path.Combine(_tempPath, @"ConfigTestAssemblies\V2_Unsigned\System.Web.WebPages.Deployment.dll"));
                Assert.Equal(new Version(2, 0, 0, 0), asm.GetName().Version);
                Assert.Equal("System.Web.WebPages.Deployment", asm.GetName().Name);

                using (WebUtils.CreateHttpRuntime(@"~\foo", "."))
                {
                    // Act
                    Version ver = WebPagesDeployment.GetVersionWithoutEnabledCheck(Path.Combine(_tempPath, @"ConfigTestSites\CshtmlFileNoVersion"));

                    // Assert
                    Assert.Equal(MaxVersion, ver);
                }
            });
        }

        [Fact]
        public void GetVersionReturnsVMaxAssemblyVersionIfCshtmlFilePresent()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange - Load v2 Config
                Assembly asm = Assembly.LoadFrom(Path.Combine(_tempPath, @"ConfigTestAssemblies\V2_Signed\System.Web.WebPages.Deployment.dll"));
                Assert.Equal(new Version(2, 0, 0, 0), asm.GetName().Version);
                Assert.Equal("System.Web.WebPages.Deployment", asm.GetName().Name);

                using (WebUtils.CreateHttpRuntime(@"~\foo", "."))
                {
                    // Act
                    Version ver = WebPagesDeployment.GetVersionWithoutEnabledCheck(Path.Combine(_tempPath, @"ConfigTestSites\CshtmlFileNoVersion"));

                    // Assert
                    Assert.Equal(MaxVersion, ver);
                }
            });
        }

        [Fact]
        public void GetVersionThrowsIfPathNullOrEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => WebPagesDeployment.GetVersionWithoutEnabledCheck(null), "path");
            Assert.ThrowsArgumentNullOrEmptyString(() => WebPagesDeployment.GetVersionWithoutEnabledCheck(String.Empty), "path");
        }

        [Fact]
        public void IsEnabledThrowsIfPathNullOrEmpty()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => WebPagesDeployment.IsEnabled(null), "path");
            Assert.ThrowsArgumentNullOrEmptyString(() => WebPagesDeployment.IsEnabled(String.Empty), "path");
        }

        [Theory]
        [InlineData(new object[] { null })]
        [InlineData(new object[] { "" })]
        public void ObsoleteGetVersionThrowsIfPathIsNullOrEmpty(string path)
        {
            // Arrange
            var fileSystem = new TestFileSystem();
            var configuration = new NameValueCollection();

            // Act and Assert
            Assert.ThrowsArgumentNullOrEmptyString(() => WebPagesDeployment.GetObsoleteVersionInternal(path, configuration, fileSystem, () => new Version("2.0.0.0")), "path");
        }

        [Fact]
        public void ObsoleteGetVersionReturnsNullIfNoFilesInTheSite()
        {
            // Arrange
            var path = "blah";
            var fileSystem = new TestFileSystem();
            var configuration = new NameValueCollection();

            // Act
            var version = WebPagesDeployment.GetObsoleteVersionInternal(path, configuration, fileSystem, () => new Version("2.0.0.0"));

            // Assert
            Assert.Null(version);
        }

        [Fact]
        public void ObsoleteGetVersionReturnsMaxVersionIfNoValueInConfigNoFilesInBinSiteContainsCshtmlFiles()
        {
            // Arrange
            var path = "blah";
            var fileSystem = new TestFileSystem();
            fileSystem.AddFile(@"blah\Foo.cshtml");
            var configuration = new NameValueCollection();

            // Act
            var version = WebPagesDeployment.GetObsoleteVersionInternal(path, configuration, fileSystem, () => new Version("2.0.0.0"));

            // Assert
            Assert.Equal(new Version("2.0.0.0"), version);
        }

        [Fact]
        public void ObsoleteGetVersionReturnsVersionFromConfigIfDisabled()
        {
            // Arrange
            var maxVersion = new Version("2.1.3.4");
            var fileSystem = new TestFileSystem();
            var configuration = new NameValueCollection();
            configuration["webPages:Enabled"] = "False";
            configuration["webPages:Version"] = "2.0";
            var path = "blah";

            // Act
            var version = WebPagesDeployment.GetObsoleteVersionInternal(path, configuration, fileSystem, () => maxVersion);

            // Assert
            Assert.Equal(new Version("2.0.0.0"), version);
        }

        private static string GetTempPath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }
    }
}
