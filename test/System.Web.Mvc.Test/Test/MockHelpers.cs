// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Moq;
using Moq.Language.Flow;

namespace System.Web.Mvc.Test
{
    public static class MockHelpers
    {
        public static ISetup<HttpContextBase> ExpectMvcVersionResponseHeader(this Mock<HttpContextBase> mock)
        {
            return mock.Setup(r => r.Response.AppendHeader(MvcHandler.MvcVersionHeaderName, "4.0"));
        }
    }
}
