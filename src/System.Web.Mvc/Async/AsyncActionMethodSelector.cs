// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc.Async
{
    internal sealed class AsyncActionMethodSelector
    {
        // This flag controls async action binding for backwards compat since Controller now supports async. 
        // Set to true for classes that derive from AsyncController. In this case, FooAsync/FooCompleted is 
        // bound as a single async action pair "Foo". If false, they're bound as 2 separate sync actions. 
        // Practically, if this is false, then IsAsyncSuffixedMethod and IsCompeltedSuffixedMethod return false.
        private readonly bool _allowLegacyAsyncActions;

        public AsyncActionMethodSelector(Type controllerType, bool allowLegacyAsyncActions = true)
        {
            _allowLegacyAsyncActions = allowLegacyAsyncActions;
            ControllerType = controllerType;
            PopulateLookupTables();
        }

        public Type ControllerType { get; private set; }

        public MethodInfo[] AliasedMethods { get; private set; }

        public ILookup<string, MethodInfo> NonAliasedMethods { get; private set; }

        private AmbiguousMatchException CreateAmbiguousActionMatchException(IEnumerable<MethodInfo> ambiguousMethods, string actionName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                                           actionName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        private AmbiguousMatchException CreateAmbiguousMethodMatchException(IEnumerable<MethodInfo> ambiguousMethods, string methodName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionMethodSelector_AmbiguousMethodMatch,
                                           methodName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        private static string CreateAmbiguousMatchList(IEnumerable<MethodInfo> ambiguousMethods)
        {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods)
            {
                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, methodInfo, methodInfo.DeclaringType.FullName);
            }

            return exceptionMessageBuilder.ToString();
        }

        public ActionDescriptorCreator FindAction(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (controllerContext.RouteData != null)
            {
                MethodInfo target = controllerContext.RouteData.GetTargetActionMethod();
                if (target != null)
                {
                    // short circuit the selection process if a direct route was matched.
                    return GetActionDescriptorDelegate(target);
                }
            }

            List<MethodInfo> finalMethods = ActionMethodSelector.FindActionMethods(controllerContext, actionName, AliasedMethods, NonAliasedMethods);
            
            switch (finalMethods.Count)
            {
                case 0:
                    return null;

                case 1:
                    MethodInfo entryMethod = finalMethods[0];
                    return GetActionDescriptorDelegate(entryMethod);

                default:
                    throw CreateAmbiguousActionMatchException(finalMethods, actionName);
            }
        }

        internal bool AllowLegacyAsyncActions
        {
            get { return _allowLegacyAsyncActions; }
        }

        private ActionDescriptorCreator GetActionDescriptorDelegate(MethodInfo entryMethod)
        {
            // Does the action return a Task?
            if (entryMethod.ReturnType != null && typeof(Task).IsAssignableFrom(entryMethod.ReturnType))
            {
                return (actionName, controllerDescriptor) => new TaskAsyncActionDescriptor(entryMethod, actionName, controllerDescriptor);
            }

            // Is this the FooAsync() / FooCompleted() pattern?
            if (IsAsyncSuffixedMethod(entryMethod))
            {
                string completionMethodName = entryMethod.Name.Substring(0, entryMethod.Name.Length - "Async".Length) + "Completed";
                MethodInfo completionMethod = GetMethodByName(completionMethodName);
                if (completionMethod != null)
                {
                    return (actionName, controllerDescriptor) => new ReflectedAsyncActionDescriptor(entryMethod, completionMethod, actionName, controllerDescriptor);
                }
                else
                {
                    throw Error.AsyncActionMethodSelector_CouldNotFindMethod(completionMethodName, ControllerType);
                }
            }

            // Fallback to synchronous method
            return (actionName, controllerDescriptor) => new ReflectedActionDescriptor(entryMethod, actionName, controllerDescriptor);
        }

        private string GetCanonicalMethodName(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            return (IsAsyncSuffixedMethod(methodInfo))
                       ? methodName.Substring(0, methodName.Length - "Async".Length)
                       : methodName;
        }

        private bool IsAsyncSuffixedMethod(MethodInfo methodInfo)
        {
            return _allowLegacyAsyncActions && methodInfo.Name.EndsWith("Async", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsCompletedSuffixedMethod(MethodInfo methodInfo)
        {
            return _allowLegacyAsyncActions && methodInfo.Name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        private MethodInfo GetMethodByName(string methodName)
        {
            List<MethodInfo> methods = (from MethodInfo methodInfo in ControllerType.GetMember(methodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase)
                                        where IsValidActionMethod(methodInfo, false /* stripInfrastructureMethods */)
                                        select methodInfo).ToList();

            switch (methods.Count)
            {
                case 0:
                    return null;

                case 1:
                    return methods[0];

                default:
                    throw CreateAmbiguousMethodMatchException(methods, methodName);
            }
        }

        private bool IsValidActionMethod(MethodInfo methodInfo)
        {
            return IsValidActionMethod(methodInfo, true /* stripInfrastructureMethods */);
        }

        private bool IsValidActionMethod(MethodInfo methodInfo, bool stripInfrastructureMethods)
        {
            if (methodInfo.IsSpecialName)
            {
                // not a normal method, e.g. a constructor or an event
                return false;
            }

            if (methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(AsyncController)))
            {
                // is a method on Object, ControllerBase, Controller, or AsyncController
                return false;
            }

            if (stripInfrastructureMethods)
            {
                if (IsCompletedSuffixedMethod(methodInfo))
                {
                    // do not match FooCompleted() methods, as these are infrastructure methods
                    return false;
                }
            }

            return true;
        }

        private void PopulateLookupTables()
        {
            MethodInfo[] allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            AliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            NonAliasedMethods = actionMethods.Except(AliasedMethods).ToLookup(GetCanonicalMethodName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
