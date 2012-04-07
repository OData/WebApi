// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpControllerDescriptorTest
    {
        [Fact]
        public void Default_Constructor()
        {
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Null(controllerDescriptor.ControllerName);
            Assert.Null(controllerDescriptor.Configuration);
            Assert.Null(controllerDescriptor.ControllerType);
            Assert.Null(controllerDescriptor.HttpActionInvoker);
            Assert.Null(controllerDescriptor.HttpActionSelector);
            Assert.Null(controllerDescriptor.HttpControllerActivator);
            Assert.NotNull(controllerDescriptor.Properties);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            HttpConfiguration config = new HttpConfiguration();
            string controllerName = "UsersController";
            Type controllerType = typeof(UsersController);

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, controllerName, controllerType);
            Assert.NotNull(controllerDescriptor.ControllerName);
            Assert.NotNull(controllerDescriptor.Configuration);
            Assert.NotNull(controllerDescriptor.ControllerType);
            Assert.NotNull(controllerDescriptor.HttpActionInvoker);
            Assert.NotNull(controllerDescriptor.HttpActionSelector);
            Assert.NotNull(controllerDescriptor.HttpControllerActivator);
            Assert.NotNull(controllerDescriptor.Properties);
            Assert.Equal(config, controllerDescriptor.Configuration);
            Assert.Equal(controllerName, controllerDescriptor.ControllerName);
            Assert.Equal(controllerType, controllerDescriptor.ControllerType);
        }

        [Fact]
        public void Constructor_Throws_IfConfigurationIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(null, "UsersController", typeof(UsersController)),
                "configuration");
        }

        [Fact]
        public void Constructor_Throws_IfControllerNameIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(new HttpConfiguration(), null, typeof(UsersController)),
                "controllerName");
        }

        [Fact]
        public void Constructor_Throws_IfControllerTypeIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new HttpControllerDescriptor(new HttpConfiguration(), "UsersController", null),
                "controllerType");
        }

        [Fact]
        public void Configuration_Property()
        {
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, HttpConfiguration>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.Configuration,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: config);
        }

        [Fact]
        public void ControllerName_Property()
        {
            string controllerName = "UsersController";
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, string>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.ControllerName,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: controllerName);
        }

        [Fact]
        public void ControllerType_Property()
        {
            Type controllerType = typeof(UsersController);
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, Type>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.ControllerType,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: controllerType);
        }

        [Fact]
        public void HttpActionInvoker_Property()
        {
            IHttpActionInvoker invoker = new ApiControllerActionInvoker();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, IHttpActionInvoker>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.HttpActionInvoker,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: invoker);
        }

        [Fact]
        public void HttpActionSelector_Property()
        {
            IHttpActionSelector selector = new ApiControllerActionSelector();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, IHttpActionSelector>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.HttpActionSelector,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: selector);
        }

        [Fact]
        public void HttpControllerActivator_Property()
        {
            IHttpControllerActivator activator = new Mock<IHttpControllerActivator>().Object;
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, IHttpControllerActivator>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.HttpControllerActivator,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: activator);
        }

        [Fact]
        public void ActionValueBinder_Property()
        {
            IActionValueBinder activator = new Mock<IActionValueBinder>().Object;
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<HttpControllerDescriptor, IActionValueBinder>(
                instance: controllerDescriptor,
                propertyGetter: cd => cd.ActionValueBinder,
                expectedDefaultValue: null,
                allowNull: false,
                roundTripTestValue: activator);
        }

        [Fact]
        public void GetFilters_InvokesGetCustomAttributesMethod()
        {
            var descriptorMock = new Mock<HttpControllerDescriptor> { CallBase = true };
            var filters = new Collection<IFilter>(new List<IFilter>());
            descriptorMock.Setup(d => d.GetCustomAttributes<IFilter>()).Returns(filters).Verifiable();

            var result = descriptorMock.Object.GetFilters();

            Assert.Same(filters, result);
            descriptorMock.Verify();
        }
    }
}
