// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Cors;
using Microsoft.TestCommon;

namespace System.Web.Http.Cors.Test
{
    public class EnableCorsAttributeTest
    {
        [Fact]
        public void Default_Constructor()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            Assert.False(enableCors.SupportsCredentials);
            Assert.Empty(enableCors.ExposedHeaders);
            Assert.Empty(enableCors.Headers);
            Assert.Empty(enableCors.Methods);
            Assert.Empty(enableCors.Origins);
            Assert.Equal(-1, enableCors.PreflightMaxAge);
        }

        [Fact]
        public void SettingNullExposedHeaders_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            Assert.ThrowsArgumentNull(() =>
            {
                enableCors.ExposedHeaders = null;
            },
            "value");
        }

        [Fact]
        public void SettingNullHeaders_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            Assert.ThrowsArgumentNull(() =>
            {
                enableCors.Headers = null;
            },
            "value");
        }

        [Fact]
        public void SettingNullMethods_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            Assert.ThrowsArgumentNull(() =>
            {
                enableCors.Methods = null;
            },
            "value");
        }

        [Fact]
        public void SettingNullOrigins_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            Assert.ThrowsArgumentNull(() =>
            {
                enableCors.Origins = null;
            },
            "value");
        }

        [Fact]
        public void GetCorsPolicyAsync_DefaultPolicyValues()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.True(corsPolicy.AllowAnyHeader);
            Assert.True(corsPolicy.AllowAnyMethod);
            Assert.True(corsPolicy.AllowAnyOrigin);
            Assert.False(corsPolicy.SupportsCredentials);
            Assert.Empty(corsPolicy.ExposedHeaders);
            Assert.Empty(corsPolicy.Headers);
            Assert.Empty(corsPolicy.Methods);
            Assert.Empty(corsPolicy.Origins);
            Assert.Null(corsPolicy.PreflightMaxAge);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedSupportsCredentials()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                SupportsCredentials = true
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.True(corsPolicy.SupportsCredentials);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedPreflightMaxAge()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                PreflightMaxAge = 20
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.Equal(20, corsPolicy.PreflightMaxAge);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedExposeHeaders()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                ExposedHeaders = new[] { "foo", "bar" }
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.Equal(new List<string> { "foo", "bar" }, corsPolicy.ExposedHeaders);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedHeaders()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                Headers = new[] { "Accept", "Content-Type" }
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.Equal(new List<string> { "Accept", "Content-Type" }, corsPolicy.Headers);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedMethods()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                Methods = new[] { "GET", "Delete" }
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.Equal(new List<string> { "GET", "Delete" }, corsPolicy.Methods);
        }

        [Fact]
        public void GetCorsPolicyAsync_RetunsExpectedOrigins()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute
            {
                Origins = new[] { "http://example.com" }
            };

            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.Equal(new List<string> { "http://example.com" }, corsPolicy.Origins);
        }

        [Fact]
        public void AllowAnyHeader_IsFalse_WhenHeadersPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();
            enableCors.Headers = new[] { "foo" };
            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.False(corsPolicy.AllowAnyHeader);
        }

        [Fact]
        public void AllowAnyOrigin_IsFalse_WhenOriginsPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();
            enableCors.Origins = new[] { "http://example.com" };
            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.False(corsPolicy.AllowAnyOrigin);
        }

        [Fact]
        public void AllowAnyMethod_IsFalse_WhenMethodsPropertyIsSet()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();
            enableCors.Methods = new[] { "GET" };
            CorsPolicy corsPolicy = enableCors.GetCorsPolicyAsync(new HttpRequestMessage()).Result;

            Assert.False(corsPolicy.AllowAnyMethod);
        }

        [Fact]
        public void SettingNegativePreflightMaxAge_Throws()
        {
            EnableCorsAttribute enableCors = new EnableCorsAttribute();
            Assert.ThrowsArgumentOutOfRange(() =>
            {
                enableCors.PreflightMaxAge = -2;
            },
            "value",
            "PreflightMaxAge must be greater than or equal to 0.");
        }
    }
}