// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test
{
    public class DescriptorUtilTest
    {
        [Fact]
        public void CreateUniqueId_FromIUniquelyIdentifiable()
        {
            // Arrange
            CustomUniquelyIdentifiable custom = new CustomUniquelyIdentifiable("hello-world");

            // Act
            string retVal = DescriptorUtil.CreateUniqueId(custom);

            // Assert
            Assert.Equal("[11]hello-world", retVal);
        }

        [Fact]
        public void CreateUniqueId_FromMemberInfo()
        {
            // Arrange
            string moduleVersionId = typeof(DescriptorUtilTest).Module.ModuleVersionId.ToString();
            string metadataToken = typeof(DescriptorUtilTest).MetadataToken.ToString();
            string expected = String.Format("[{0}]{1}[{2}]{3}", moduleVersionId.Length, moduleVersionId, metadataToken.Length, metadataToken);

            // Act
            string retVal = DescriptorUtil.CreateUniqueId(typeof(DescriptorUtilTest));

            // Assert
            Assert.Equal(expected, retVal);
        }

        [Fact]
        public void CreateUniqueId_FromSimpleTypes()
        {
            // Act
            string retVal = DescriptorUtil.CreateUniqueId("foo", null, 12345);

            // Assert
            Assert.Equal("[3]foo[-1][5]12345", retVal);
        }

        private sealed class CustomUniquelyIdentifiable : IUniquelyIdentifiable
        {
            public CustomUniquelyIdentifiable(string uniqueId)
            {
                UniqueId = uniqueId;
            }

            public string UniqueId { get; private set; }
        }
    }
}
