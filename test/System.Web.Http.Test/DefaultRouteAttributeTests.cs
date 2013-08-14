// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class DefaultRouteAttributeTest
    {
        [Fact]
        public void DefaultCtor_IsEmptyString()
        {
            DefaultRouteAttribute attribute = new DefaultRouteAttribute();

            Assert.Equal(String.Empty, attribute.RouteTemplate);
        }

        [Fact]
        public void Ctor_NotNull()
        {
            Assert.ThrowsArgumentNull(() => new DefaultRouteAttribute(null), "routeTemplate");
        }
    }
}
