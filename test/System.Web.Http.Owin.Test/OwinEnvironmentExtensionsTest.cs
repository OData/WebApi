// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Http.Owin
{
    public class OwinEnvironmentExtensionsTest
    {
        [Fact]
        public void GetOwinValue_GetsValues()
        {
            var env = new Dictionary<string, object>();
            env.Add("key", "value");

            string value = env.GetOwinValue<string>("key");

            Assert.Equal("value", value);
        }

        [Fact]
        public void GetOwinValue_Throws_ForMissingKey()
        {
            var env = new Dictionary<string, object>();

            Assert.Throws<InvalidOperationException>(
                () => env.GetOwinValue<string>("key"),
                "The OWIN environment does not contain a value for the required key 'key'.");
        }

        [Fact]
        public void GetOwinValue_Throws_ForKeyWithUnexpectedType()
        {
            var env = new Dictionary<string, object>();
            env.Add("key", new object());

            Assert.Throws<InvalidOperationException>(
                () => env.GetOwinValue<string>("key"),
                "The value for key 'key' in the OWIN environment is not of the expected type 'String'.");
        }
    }
}
