// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class GenerateRouteTests
    {
        [Fact]
        public void GenerateRoute_DoesNotClaimData()
        {
            GenerateRoute route = new GenerateRoute(new NotImplementedRoute());

            IHttpRouteData data = route.GetRouteData(string.Empty, new HttpRequestMessage());

            Assert.Null(data);            
        }

        [Fact]
        public void GenerateRoute_EmptyProperties()
        {
            GenerateRoute route = new GenerateRoute(new NotImplementedRoute());

            AssertDictionaryIsEmptyAndImmutable(route.Defaults);
            AssertDictionaryIsEmptyAndImmutable(route.Constraints);
            AssertDictionaryIsEmptyAndImmutable(route.DataTokens);
            Assert.Equal(null, route.Handler);
            Assert.Equal(string.Empty, route.RouteTemplate);
        }

        [Fact]
        public void GenerateRoute_GetVirtualPathIsForwarded()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            IDictionary<string, object> values = new Dictionary<string, object>();

            IHttpVirtualPathData data = new Mock<IHttpVirtualPathData>().Object;

            Mock<IHttpRoute> inner = new Mock<IHttpRoute>();
            inner.Setup(r => r.GetVirtualPath(request, values)).Returns(data);

            GenerateRoute route = new GenerateRoute(inner.Object);

            IHttpVirtualPathData result = route.GetVirtualPath(request, values);

            Assert.Equal(data, result);        
        }

        static void AssertDictionaryIsEmptyAndImmutable<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Assert.Equal(0, dict.Count);

            // Verify it's immutable by trying to add an object. 
            // The exact exception thrown is not important. 
            Assert.Throws<NotSupportedException>(() => dict[default(TKey)] = default(TValue));
        }

        // Route where everything is not implemented. Tests that the generated route is not forwarding calls. 
        private class NotImplementedRoute : IHttpRoute
        {
            public string RouteTemplate
            {
                get { throw new NotImplementedException(); }
            }

            public Collections.Generic.IDictionary<string, object> Defaults
            {
                get { throw new NotImplementedException(); }
            }

            public Collections.Generic.IDictionary<string, object> Constraints
            {
                get { throw new NotImplementedException(); }
            }

            public Collections.Generic.IDictionary<string, object> DataTokens
            {
                get { throw new NotImplementedException(); }
            }

            public HttpMessageHandler Handler
            {
                get { throw new NotImplementedException(); }
            }

            public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }

            public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, Collections.Generic.IDictionary<string, object> values)
            {
                throw new NotImplementedException();
            }
        }
    }
}
