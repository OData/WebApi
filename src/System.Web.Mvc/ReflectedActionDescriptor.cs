// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc
{
    public class ReflectedActionDescriptor : ActionDescriptor
    {
        private readonly string _actionName;
        private readonly ControllerDescriptor _controllerDescriptor;
        private readonly Lazy<string> _uniqueId;
        private ParameterDescriptor[] _parametersCache;

        public ReflectedActionDescriptor(MethodInfo methodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : this(methodInfo, actionName, controllerDescriptor, true /* validateMethod */)
        {
        }

        internal ReflectedActionDescriptor(MethodInfo methodInfo, string actionName, ControllerDescriptor controllerDescriptor, bool validateMethod)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            if (String.IsNullOrEmpty(actionName))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            if (validateMethod)
            {
                string failedMessage = VerifyActionMethodIsCallable(methodInfo);
                if (failedMessage != null)
                {
                    throw new ArgumentException(failedMessage, "methodInfo");
                }
            }

            MethodInfo = methodInfo;
            _actionName = actionName;
            _controllerDescriptor = controllerDescriptor;
            _uniqueId = new Lazy<string>(CreateUniqueId);
        }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public override ControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
        }

        public MethodInfo MethodInfo { get; private set; }

        public override string UniqueId
        {
            get { return _uniqueId.Value; }
        }

        private string CreateUniqueId()
        {
            return base.UniqueId + DescriptorUtil.CreateUniqueId(MethodInfo);
        }

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            ParameterInfo[] parameterInfos = MethodInfo.GetParameters();
            var rawParameterValues = from parameterInfo in parameterInfos
                                     select ExtractParameterFromDictionary(parameterInfo, parameters, MethodInfo);
            object[] parametersArray = rawParameterValues.ToArray();

            ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(MethodInfo);
            object actionReturnValue = dispatcher.Execute(controllerContext.Controller, parametersArray);
            return actionReturnValue;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(MethodInfo, inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.GetCustomAttributes(MethodInfo, attributeType, inherit);
        }

        public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
        {
            if (useCache && GetType() == typeof(ReflectedActionDescriptor))
            {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetMethodFilterAttributes(MethodInfo);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override ParameterDescriptor[] GetParameters()
        {
            return ActionDescriptorHelper.GetParameters(this, MethodInfo, ref _parametersCache);
        }

        public override ICollection<ActionSelector> GetSelectors()
        {
            return ActionDescriptorHelper.GetSelectors(MethodInfo);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ActionDescriptorHelper.IsDefined(MethodInfo, attributeType, inherit);
        }

        internal static ReflectedActionDescriptor TryCreateDescriptor(MethodInfo methodInfo, string name, ControllerDescriptor controllerDescriptor)
        {
            ReflectedActionDescriptor descriptor = new ReflectedActionDescriptor(methodInfo, name, controllerDescriptor, false /* validateMethod */);
            string failedMessage = VerifyActionMethodIsCallable(methodInfo);
            return (failedMessage == null) ? descriptor : null;
        }
    }
}
