// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Mvc
{
    internal sealed class ActionMethodSelector : ActionMethodSelectorBase
    {
        public ActionMethodSelector(Type controllerType)            
        {
            Initialize(controllerType);
        }
                
        protected override bool IsValidActionMethod(MethodInfo methodInfo)
        {
            return !(methodInfo.IsSpecialName ||
                     methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
        }        
    }
}
