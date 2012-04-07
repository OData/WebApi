// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.WebHost
{
    public class SuppressFormsAuthRedirectModuleTest
    {
        [Fact]
        public void DisableAuthenticationRedirect_SetTheFlagToTrue()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() {DefaultValue = DefaultValue.Mock};
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            // Act
            SuppressFormsAuthRedirectModule.DisableAuthenticationRedirect(contextMock.Object);

            // Assert
            AssertEx.True(contextItems.Contains(SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey));
            AssertEx.True((bool) contextItems[SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey]);
        }

        [Fact]
        public void EnableAuthenticationRedirect_SetTheFlagToFalse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            // Act
            SuppressFormsAuthRedirectModule.AllowAuthenticationRedirect(contextMock.Object);

            // Assert
            AssertEx.True(contextItems.Contains(SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey));
            AssertEx.False((bool) contextItems[SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey]);
        }

        [Fact]
        public void OnEndRequest_IfDisableRedirectAndStatusIsRedirect_ModifyResponse()
        {
            // Arrange
            HttpResponse response = new HttpResponse(null);
            IDictionary contextItems = new Hashtable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            DisableRedirectStub disableRedirectStub = new DisableRedirectStub(contextItems, response);

            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);
            response.StatusCode = 302;

            // Act
            SuppressFormsAuthRedirectModule.DisableAuthenticationRedirect(contextMock.Object);
            SuppressFormsAuthRedirectModule.EnsureRestoreUnauthorized(disableRedirectStub);

            // Assert
            AssertEx.Equal(401, response.StatusCode);
        }

        [Fact]
        public void OnEndRequest_IfDisableRedirectAndStatusIsNotRedirect_DoNothing()
        {
            // Arrange
            HttpResponse response = new HttpResponse(null);
            IDictionary contextItems = new Hashtable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            DisableRedirectStub disableRedirectStub = new DisableRedirectStub(contextItems, response);

            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);
            response.StatusCode = 200;

            // Act
            SuppressFormsAuthRedirectModule.DisableAuthenticationRedirect(contextMock.Object);
            SuppressFormsAuthRedirectModule.EnsureRestoreUnauthorized(disableRedirectStub);

            // Assert
            AssertEx.Equal(200, response.StatusCode);
        }

        [Fact]
        public void OnEndRequest_IfEnableRedirectAndStatusIsRedirect_DoNothing()
        {
            // Arrange
            HttpResponse response = new HttpResponse(null);
            IDictionary contextItems = new Hashtable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            DisableRedirectStub disableRedirectStub = new DisableRedirectStub(contextItems, response);

            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);
            response.StatusCode = 302;

            // Act
            SuppressFormsAuthRedirectModule.AllowAuthenticationRedirect(contextMock.Object);
            SuppressFormsAuthRedirectModule.EnsureRestoreUnauthorized(disableRedirectStub);

            // Assert
            AssertEx.Equal(302, response.StatusCode);
        }


        [Fact]
        public void OnEndRequest_IfWebApiControllerReturnsARedirect_DoNothing() {
            // Arrange
            HttpResponse response = new HttpResponse(null);
            IDictionary contextItems = new Hashtable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            DisableRedirectStub disableRedirectStub = new DisableRedirectStub(contextItems, response);

            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);
            response.StatusCode = 302;

            // Act
            HttpControllerHandler.EnsureSuppressFormsAuthenticationRedirect(contextMock.Object);

            SuppressFormsAuthRedirectModule.AllowAuthenticationRedirect(contextMock.Object);
            SuppressFormsAuthRedirectModule.EnsureRestoreUnauthorized(disableRedirectStub);

            // Assert
            AssertEx.Equal(302, response.StatusCode);
        }

        [Fact]
        public void OnEndRequest_IfEnableRedirectAndStatusIsNotRedirect_DoNothing()
        {
            // Arrange
            HttpResponse response = new HttpResponse(null);
            IDictionary contextItems = new Hashtable();
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            DisableRedirectStub disableRedirectStub = new DisableRedirectStub(contextItems, response);

            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);
            response.StatusCode = 200;

            // Act
            SuppressFormsAuthRedirectModule.AllowAuthenticationRedirect(contextMock.Object);
            SuppressFormsAuthRedirectModule.EnsureRestoreUnauthorized(disableRedirectStub);

            // Assert
            AssertEx.Equal(200, response.StatusCode);
        }

        [Theory]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("", true)]
        [InlineData("foo", true)]
        public void GetDisabled_ParsesAppSettings(string setting, bool expected)
        {
            AssertEx.Equal(expected, SuppressFormsAuthRedirectModule.GetEnabled(new NameValueCollection() { { SuppressFormsAuthRedirectModule.AppSettingsSuppressFormsAuthenticationRedirectKey, setting } }));
        }
        

        [Fact]
        public void PreApplicationStartCode_IsValid() 
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }

    internal class DisableRedirectStub : SuppressFormsAuthRedirectModule.IDisableRedirect {
        private readonly IDictionary _contextItems;
        private readonly HttpResponse _response;

        public DisableRedirectStub(IDictionary contextItems, HttpResponse response) {
            _contextItems = contextItems;
            _response = response;
        }

        public IDictionary ContextItems {
            get { return _contextItems; }
        }

        public HttpResponse Response {
            get { return _response; }
        }
    }
}