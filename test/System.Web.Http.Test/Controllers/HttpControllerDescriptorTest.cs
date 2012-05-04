// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Formatting;
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
        public void GetFilters_InvokesGetCustomAttributesMethod()
        {
            var descriptorMock = new Mock<HttpControllerDescriptor> { CallBase = true };
            var filters = new Collection<IFilter>(new List<IFilter>());
            descriptorMock.Setup(d => d.GetCustomAttributes<IFilter>()).Returns(filters).Verifiable();

            var result = descriptorMock.Object.GetFilters();

            Assert.Same(filters, result);
            descriptorMock.Verify();
        }

        [Fact]
        public void Initialize_In_InheritenceHierarchy()
        {
            // Verifies that initialization is run in order with , and that they all mutate on the same descriptor object 
            HttpConfiguration config = new HttpConfiguration();

            // Act.
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyDerived2Controller));

            // Assert
            Assert.Same(MyDerived1Controller.SelectorBase, desc.ControllerServices.GetActionSelector());
            Assert.Same(MyDerived1Controller.ActionValueBinderDerived1, desc.ControllerServices.GetActionValueBinder());
            Assert.Same(config.Formatters, desc.Formatters); // didn't override, stays the same
            Assert.Same(config.ParameterBindingRules, desc.ParameterBindingRules); // didn't override, stays the same
        }

        class MyConfigBaseAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerDescriptor desc) 
            {
                desc.ControllerServices.Replace(typeof(IActionValueBinder), MyBaseController.ActionValueBinderBase);
                desc.ControllerServices.Replace(typeof(IHttpActionSelector), MyBaseController.SelectorBase);
            }
        }

        [MyConfigBase]
        class MyBaseController : ApiController
        {
            public static IHttpActionSelector SelectorBase = new Mock<IHttpActionSelector>().Object;
            public static IActionValueBinder ActionValueBinderBase = new Mock<IActionValueBinder>().Object;
        }

        class MyConfigDerived1Attribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerDescriptor desc)
            {
                // Base runs first, so we should be able to see changes from the base.
                Assert.Same(MyBaseController.ActionValueBinderBase, desc.ControllerServices.GetActionValueBinder());

                // Also overwrite them
                desc.ControllerServices.Replace(typeof(IActionValueBinder), MyDerived1Controller.ActionValueBinderDerived1);
            }
        }

        [MyConfigDerived1]
        class MyDerived1Controller : MyBaseController
        {
            public static IActionValueBinder ActionValueBinderDerived1 = new Mock<IActionValueBinder>().Object;
        }

        class MyDerived2Controller : MyDerived1Controller
        {
        }


        [Fact]
        public void Initialize_Append_A_Formatter()
        {
            // Verifies that controller inherit the formatter list from the global config, and can mutate it. 
            HttpConfiguration config = new HttpConfiguration();

            MediaTypeFormatter globalFormatter = new Mock<MediaTypeFormatter>().Object;
            config.Formatters.Clear();
            config.Formatters.Add(globalFormatter);

            // Act.
            HttpControllerDescriptor desc = new HttpControllerDescriptor(config, "MyController", typeof(MyControllerWithCustomFormatter));

            // Assert
            Assert.Equal(2, desc.Formatters.Count);
            Assert.Same(globalFormatter, desc.Formatters[0]);
            Assert.Same(MyControllerWithCustomFormatter.CustomFormatter, desc.Formatters[1]);
        }


        class MyControllerWithCustomFormatterConfigAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerDescriptor desc)
            {
                // Appends to existing list. Formatter list has copy-on-write semantics. 
                Assert.Equal(1, desc.Formatters.Count); // the one we already set 
                desc.Formatters.Add(MyControllerWithCustomFormatter.CustomFormatter);
            }
        }

        [MyControllerWithCustomFormatterConfig]
        class MyControllerWithCustomFormatter : ApiController
        {
            public static MediaTypeFormatter CustomFormatter = new Mock<MediaTypeFormatter>().Object;

        }

    }
}
