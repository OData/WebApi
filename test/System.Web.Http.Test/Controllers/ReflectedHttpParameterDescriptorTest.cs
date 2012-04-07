// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class ReflectedHttpParameterDescriptorTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            UsersRpcController controller = new UsersRpcController();
            Func<string, string, User> echoUserMethod = controller.EchoUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = echoUserMethod.Method };
            ParameterInfo parameterInfo = echoUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);

            Assert.Equal(actionDescriptor, parameterDescriptor.ActionDescriptor);
            Assert.Null(parameterDescriptor.DefaultValue);
            Assert.Equal(parameterInfo, parameterDescriptor.ParameterInfo);
            Assert.Equal(parameterInfo.Name, parameterDescriptor.ParameterName);
            Assert.Equal(typeof(string), parameterDescriptor.ParameterType);
            Assert.Null(parameterDescriptor.Prefix);
            Assert.Null(parameterDescriptor.ModelBinderAttribute);
        }

        [Fact]
        public void Constructor_Throws_IfParameterInfoIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpParameterDescriptor(new Mock<HttpActionDescriptor>().Object, null),
                "parameterInfo");
        }

        [Fact]
        public void Constructor_Throws_IfActionDescriptorIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => new ReflectedHttpParameterDescriptor(null, new Mock<ParameterInfo>().Object),
                "actionDescriptor");
        }

        [Fact]
        public void ParameterInfo_Property()
        {
            ParameterInfo referenceParameter = new Mock<ParameterInfo>().Object;
            Assert.Reflection.Property(new ReflectedHttpParameterDescriptor(), d => d.ParameterInfo, expectedDefaultValue: null, allowNull: false, roundTripTestValue: referenceParameter);
        }

        [Fact]
        public void IsDefined_Retruns_True_WhenParameterAttributeIsFound()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
        }

        [Fact]
        public void GetCustomAttributes_Returns_ParameterAttributes()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            object[] attributes = parameterDescriptor.GetCustomAttributes<object>().ToArray();

            Assert.Equal(1, attributes.Length);
            Assert.Equal(typeof(FromBodyAttribute), attributes[0].GetType());
        }

        [Fact]
        public void GetCustomAttributes_AttributeType_Returns_ParameterAttributes()
        {
            UsersRpcController controller = new UsersRpcController();
            Action<User> addUserMethod = controller.AddUser;
            ReflectedHttpActionDescriptor actionDescriptor = new ReflectedHttpActionDescriptor { MethodInfo = addUserMethod.Method };
            ParameterInfo parameterInfo = addUserMethod.Method.GetParameters()[0];
            ReflectedHttpParameterDescriptor parameterDescriptor = new ReflectedHttpParameterDescriptor(actionDescriptor, parameterInfo);
            IEnumerable<FromBodyAttribute> attributes = parameterDescriptor.GetCustomAttributes<FromBodyAttribute>();

            Assert.Equal(1, attributes.Count());
        }
    }
}
