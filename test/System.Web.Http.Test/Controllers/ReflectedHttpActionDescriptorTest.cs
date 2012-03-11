using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class ReflectedHttpActionDescriptorTest
    {
        [Fact]
        public void Default_Constructor()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();

            Assert.Null(actionDescriptor.ActionName);
            Assert.Null(actionDescriptor.Configuration);
            Assert.Null(actionDescriptor.ControllerDescriptor);
            Assert.Null(actionDescriptor.MethodInfo);
            Assert.Null(actionDescriptor.ReturnType);
            Assert.NotNull(actionDescriptor.Properties);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(config, "", typeof(UsersRpcController));
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, echoUserMethod.Method);

            Assert.Equal("EchoUser", actionDescriptor.ActionName);
            Assert.Equal(config, actionDescriptor.Configuration);
            Assert.Equal(typeof(UsersRpcController), actionDescriptor.ControllerDescriptor.ControllerType);
            Assert.Equal(echoUserMethod.Method, actionDescriptor.MethodInfo);
            Assert.Equal(typeof(User), actionDescriptor.ReturnType);
            Assert.NotNull(actionDescriptor.Properties);
        }

        [Fact]
        public void Constructor_Throws_IfMethodInfoIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpActionDescriptor(new HttpControllerDescriptor(), null),
                "methodInfo");
        }

        [Fact]
        public void MethodInfo_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            Action action = new Action(() => { });

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, MethodInfo>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.MethodInfo,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: action.Method);
        }

        [Fact]
        public void ControllerDescriptor_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor();

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, HttpControllerDescriptor>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.ControllerDescriptor,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: controllerDescriptor);
        }

        [Fact]
        public void Configuration_Property()
        {
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor();
            HttpConfiguration config = new HttpConfiguration();

            Assert.Reflection.Property<ReflectedHttpActionDescriptor, HttpConfiguration>(
                 instance: actionDescriptor,
                 propertyGetter: ad => ad.Configuration,
                 expectedDefaultValue: null,
                 allowNull: false,
                 roundTripTestValue: config);
        }

        [Fact]
        public void GetFilter_Returns_AttributedFilter()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.AddAdmin;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>
            {
                {"firstName", "test"},
                {"lastName", "unit"}
            };

            IEnumerable<IFilter> filters = actionDescriptor.GetFilters();

            Assert.NotNull(filters);
            Assert.Equal(1, filters.Count());
            Assert.Equal(typeof(AuthorizeAttribute), filters.First().GetType());
        }

        [Fact]
        public void GetFilterPipeline_Returns_ConfigurationFilters()
        {
            IActionFilter actionFilter = new Mock<IActionFilter>().Object;
            IExceptionFilter exceptionFilter = new Mock<IExceptionFilter>().Object;
            IAuthorizationFilter authorizationFilter = new AuthorizeAttribute();
            UsersRpcController controller = new UsersRpcController();
            Action deleteAllUsersMethod = controller.DeleteAllUsers;

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "UsersRpcController", typeof(UsersRpcController));
            controllerDescriptor.Configuration.Filters.Add(actionFilter);
            controllerDescriptor.Configuration.Filters.Add(exceptionFilter);
            controllerDescriptor.Configuration.Filters.Add(authorizationFilter);
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, deleteAllUsersMethod.Method);

            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();

            Assert.Same(actionFilter, filters[0].Instance);
            Assert.Same(exceptionFilter, filters[1].Instance);
            Assert.Same(authorizationFilter, filters[2].Instance);
        }

        [Fact]
        public void GetCustomAttributes_Returns_ActionAttributes()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.AddAdmin;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);

            IEnumerable<IFilter> filters = actionDescriptor.GetCustomAttributes<IFilter>();
            IEnumerable<HttpGetAttribute> httpGet = actionDescriptor.GetCustomAttributes<HttpGetAttribute>();

            Assert.NotNull(filters);
            Assert.Equal(1, filters.Count());
            Assert.Equal(typeof(AuthorizeAttribute), filters.First().GetType());
            Assert.NotNull(httpGet);
            Assert.Equal(1, httpGet.Count());
        }

        [Fact]
        public void GetParameters_Returns_ActionParameters()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);

            Collection<HttpParameterDescriptor> parameterDescriptors = actionDescriptor.GetParameters();

            Assert.Equal(2, parameterDescriptors.Count);
            Assert.NotNull(parameterDescriptors.Where(p => p.ParameterName == "firstName").FirstOrDefault());
            Assert.NotNull(parameterDescriptors.Where(p => p.ParameterName == "lastName").FirstOrDefault());
        }

        [Fact]
        public void Execute_Returns_Null_ForVoidAction()
        {
            UsersRpcController controller = new UsersRpcController();
            Action deleteAllUsersMethod = controller.DeleteAllUsers;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = deleteAllUsersMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>();

            object returnValue = actionDescriptor.Execute(context, arguments);

            Assert.Null(returnValue);
        }

        [Fact]
        public void Execute_Returns_Results_ForNonVoidAction()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>
            {
                {"firstName", "test"},
                {"lastName", "unit"}
            };

            User returnValue = actionDescriptor.Execute(context, arguments) as User;

            Assert.NotNull(returnValue);
            Assert.Equal("test", returnValue.FirstName);
            Assert.Equal("unit", returnValue.LastName);
        }

        [Fact]
        public void Execute_Throws_IfContextIsNull()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            Dictionary<string, object> arguments = new Dictionary<string, object>();

            Assert.ThrowsArgumentNull(
                () => actionDescriptor.Execute(null, arguments),
                "controllerContext");
        }

        [Fact]
        public void Execute_Throws_IfArgumentsIsNull()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext();

            Assert.ThrowsArgumentNull(
                () => actionDescriptor.Execute(context, null),
                "arguments");
        }

        [Fact]
        public void Execute_Throws_IfValueTypeArgumentsIsNull()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<int, User> retrieveUserMethod = controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>
            {
                {"id", null}
            };

            Assert.Throws<HttpResponseException>(
                () => actionDescriptor.Execute(context, arguments),
                Error.Format(
                    SRResources.ReflectedActionDescriptor_ParameterCannotBeNull,
                    "id",
                    typeof(int),
                    actionDescriptor.MethodInfo,
                    controller.GetType()));
        }

        [Fact]
        public void Execute_Throws_IfArgumentNameIsWrong()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<int, User> retrieveUserMethod = controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>
            {
                {"otherId", 6}
            };

            Assert.Throws<HttpResponseException>(
                () => actionDescriptor.Execute(context, arguments),
                Error.Format(
                    SRResources.ReflectedActionDescriptor_ParameterNotInDictionary,
                    "id",
                    typeof(int),
                    actionDescriptor.MethodInfo,
                    controller.GetType()));
        }

        [Fact]
        public void Execute_Throws_IfArgumentTypeIsWrong()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<int, User> retrieveUserMethod = controller.RetriveUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = retrieveUserMethod.Method };
            HttpControllerContext context = ContextUtil.CreateControllerContext(instance: controller);
            Dictionary<string, object> arguments = new Dictionary<string, object>
            {
                {"id", new DateTime()}
            };

            Assert.Throws<HttpResponseException>(
                () => actionDescriptor.Execute(context, arguments),
                Error.Format(
                    SRResources.ReflectedActionDescriptor_ParameterValueHasWrongType,
                    "id",
                    actionDescriptor.MethodInfo,
                    controller.GetType(),
                    typeof(DateTime),
                    typeof(int)));
        }
    }
}
