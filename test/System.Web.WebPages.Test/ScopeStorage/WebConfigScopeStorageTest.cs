// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.WebPages.Scope;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.WebPages.Test
{
    public class WebConfigScopeStorageTest
    {
        [Fact]
        public void WebConfigScopeStorageReturnsConfigValue()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.Equal(stateStorage["foo1"], "bar1");
            Assert.Equal(stateStorage["foo2"], "bar2");
        }

        [Fact]
        public void WebConfigScopeStoragePerformsCaseInsensitiveKeyCompares()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Assert
            Assert.Equal(stateStorage["FOO1"], "bar1");
            Assert.Equal(stateStorage["FoO2"], "bar2");
        }

        [Fact]
        public void WebConfigScopeStorageThrowsWhenWriting()
        {
            // Arrange
            var stateStorage = GetWebConfigScopeStorage();

            // Act and Assert
            Assert.Throws<NotSupportedException>(() => stateStorage["foo"] = "some value", "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Add("foo", "value"), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Remove("foo"), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Clear(), "Storage scope is read only.");
            Assert.Throws<NotSupportedException>(() => stateStorage.Remove(new KeyValuePair<object, object>("foo", "bar")), "Storage scope is read only.");
        }

        [Fact]
        public void WebConfigStateAllowsEnumeratingOverConfigItems()
        {
            // Arrange
            var dictionary = new Dictionary<string, string> { { "a", "b" }, { "c", "d" }, { "x12", "y34" } };
            var stateStorage = GetWebConfigScopeStorage(dictionary);

            // Act and Assert
            Assert.True(dictionary.All(item => item.Value == stateStorage[item.Key] as string));
        }

        private WebConfigScopeDictionary GetWebConfigScopeStorage(IDictionary<string, string> values = null)
        {
            NameValueCollection collection = new NameValueCollection();
            if (values == null)
            {
                collection.Add("foo1", "bar1");
                collection.Add("foo2", "bar2");
            }
            else
            {
                foreach (var item in values)
                {
                    collection.Add(item.Key, item.Value);
                }
            }

            return new WebConfigScopeDictionary(collection);
        }
    }
}
