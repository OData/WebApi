// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace System.Web.Mvc
{
    internal static class ActionDescriptorHelper
    {
        public static ICollection<ActionSelector> GetSelectors(MethodInfo methodInfo)
        {
            ActionMethodSelectorAttribute[] attrs = (ActionMethodSelectorAttribute[])methodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), inherit: true);
            ActionSelector[] selectors = Array.ConvertAll(attrs, attr => (ActionSelector)(controllerContext => attr.IsValidForRequest(controllerContext, methodInfo)));
            return selectors;
        }

        public static bool IsDefined(MemberInfo methodInfo, Type attributeType, bool inherit)
        {
            return methodInfo.IsDefined(attributeType, inherit);
        }

        public static object[] GetCustomAttributes(MemberInfo methodInfo, bool inherit)
        {
            return methodInfo.GetCustomAttributes(inherit);
        }

        public static object[] GetCustomAttributes(MemberInfo methodInfo, Type attributeType, bool inherit)
        {
            return methodInfo.GetCustomAttributes(attributeType, inherit);
        }

        public static ParameterDescriptor[] GetParameters(ActionDescriptor actionDescriptor, MethodInfo methodInfo, ref ParameterDescriptor[] parametersCache)
        {
            ParameterDescriptor[] parameters = LazilyFetchParametersCollection(actionDescriptor, methodInfo, ref parametersCache);

            // need to clone array so that user modifications aren't accidentally stored
            return (ParameterDescriptor[])parameters.Clone();
        }

        private static ParameterDescriptor[] LazilyFetchParametersCollection(ActionDescriptor actionDescriptor, MethodInfo methodInfo, ref ParameterDescriptor[] parametersCache)
        {
            return DescriptorUtil.LazilyFetchOrCreateDescriptors<ParameterInfo, ParameterDescriptor>(
                cacheLocation: ref parametersCache,
                initializer: methodInfo.GetParameters,
                converter: parameterInfo => new ReflectedParameterDescriptor(parameterInfo, actionDescriptor));
        }
    }
}
