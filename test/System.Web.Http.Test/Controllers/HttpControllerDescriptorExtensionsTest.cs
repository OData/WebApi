// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;
using System.Web.Http.Dependencies;

namespace System.Web.Http
{
    public class HttpControllerDescriptorExtensionsTest
    {
        [Fact]
        public void ReplaceService_uses_provided_instance()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor cd = new HttpControllerDescriptor(config);

            // Act
            IHttpActionInvoker service = new MyActionInvoker();
            cd.ReplaceService<IHttpActionInvoker>(service);

            // Assert
            IHttpActionInvoker  result = cd.ControllerServices.GetActionInvoker();
            Assert.Same(service, result);
            Assert.False(typeof(MyActionInvoker).IsPublic); // passing in explicit instance means we don't need a public ctor
        }

        [Fact]
        public void ReplaceService_DI_set_base_type_not_used()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor cd = new HttpControllerDescriptor(config);
            
            
            IHttpActionInvoker mockInvoker = new Mock<IHttpActionInvoker>().Object;

            var dr = new Mock<IDependencyResolver>();
            dr.Setup(x => x.GetService(typeof(IHttpActionInvoker))).Returns(mockInvoker);
            config.DependencyResolver = dr.Object;

            // Act
            IHttpActionInvoker service = new MyActionInvoker();
            cd.ReplaceService<IHttpActionInvoker>(service);

            // Assert- still picks up supplied one, because DI doesn't have a derived type
            IHttpActionInvoker result = cd.ControllerServices.GetActionInvoker();
            Assert.Same(service, result);
        }

        [Fact]
        public void ReplaceService_DI_set_derived_type_used()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor cd = new HttpControllerDescriptor(config);

            MyPublicActionInvoker mockInvoker = new Mock<MyPublicActionInvoker>().Object;

            var dr = new Mock<IDependencyResolver>();
            dr.Setup(x => x.GetService(typeof(MyPublicActionInvoker))).Returns(mockInvoker);
            config.DependencyResolver = dr.Object;

            // Act
            IHttpActionInvoker service = new MyPublicActionInvoker();
            cd.ReplaceService<IHttpActionInvoker>(service);

            // Assert- still picks up supplied one, because DI doesn't have a derived type
            IHttpActionInvoker result = cd.ControllerServices.GetActionInvoker();
            Assert.Same(mockInvoker, result);
        }

        // Don't use mocks because we need a physical type to key with 
        private class MyActionInvoker : IHttpActionInvoker
        {
            public Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        // Public version to use with mocks
        public class MyPublicActionInvoker : IHttpActionInvoker
        {
            public Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
