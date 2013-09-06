// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    // Common base class for Async and Sync action selectors
    internal abstract class ActionMethodSelectorBase
    {
        protected void Initialize(Type controllerType)            
        {
            ControllerType = controllerType;
            PopulateLookupTables();

            // If controller type has a RouteAttribute, then standard routes can't reach it.             
            _hasRouteAttributeOnController = controllerType.GetCustomAttributes(typeof(IRouteInfoProvider), inherit: false).Any();
        }

        private bool _hasRouteAttributeOnController;

        public Type ControllerType { get; private set; }

        public MethodInfo[] AliasedMethods { get; private set; }

        public ILookup<string, MethodInfo> NonAliasedMethods { get; private set; }

        public MethodInfo[] DirectRouteMethods { get; private set; }

        protected AmbiguousMatchException CreateAmbiguousActionMatchException(IEnumerable<MethodInfo> ambiguousMethods, string actionName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                                           actionName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        protected AmbiguousMatchException CreateAmbiguousMethodMatchException(IEnumerable<MethodInfo> ambiguousMethods, string methodName)
        {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionMethodSelector_AmbiguousMethodMatch,
                                           methodName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        protected static string CreateAmbiguousMatchList(IEnumerable<MethodInfo> ambiguousMethods)
        {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods)
            {
                string controllerAction = Convert.ToString(methodInfo, CultureInfo.CurrentCulture);
                string controllerType = methodInfo.DeclaringType.FullName;

                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, controllerAction, controllerType);
            }

            return exceptionMessageBuilder.ToString();
        }

        private bool IsValidActionMethodNoDirectRoute(MethodInfo methodInfo)
        {
            return IsValidActionMethod(methodInfo) && !HasDirectRoutes(methodInfo);
        }

        private bool IsValidActionMethodWithDirectRoute(MethodInfo methodInfo)
        {
            return IsValidActionMethod(methodInfo) && HasDirectRoutes(methodInfo);
        }

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        protected abstract bool IsValidActionMethod(MethodInfo methodInfo);

        // Get the method name (before applying Aliasing attributes).
        protected virtual string GetCanonicalMethodName(MethodInfo methodInfo)
        {
            string methodName = methodInfo.Name;
            return methodName;
        }
                
        // Does this method have any direct routes on it?
        // This does not include a route attribute on the controller itself.
        private bool HasDirectRoutes(MethodInfo method)
        {
            // Inherited actions should not inherit the Route attributes.
            // Only check the attribute on declared actions.
            bool isDeclaredAction = method.DeclaringType == ControllerType;
            return isDeclaredAction && method.GetCustomAttributes(typeof(IRouteInfoProvider), inherit: false).Any();
        }

        private void PopulateLookupTables()
        {
            MethodInfo[] allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethodNoDirectRoute);

            AliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            NonAliasedMethods = actionMethods.Except(AliasedMethods).ToLookup(GetCanonicalMethodName, StringComparer.OrdinalIgnoreCase);

            DirectRouteMethods = Array.FindAll(allMethods, IsValidActionMethodWithDirectRoute);
        }

        protected List<MethodInfo> FindActionMethods(ControllerContext controllerContext, string actionName)
        {
            List<MethodInfo> matches = new List<MethodInfo>();

            // Performance sensitive, so avoid foreach
            for (int i = 0; i < AliasedMethods.Length; i++)
            {
                MethodInfo method = AliasedMethods[i];
                if (IsMatchingAliasedMethod(method, controllerContext, actionName))
                {
                    matches.Add(method);
                }
            }
            matches.AddRange(NonAliasedMethods[actionName]);
            RunSelectionFilters(controllerContext, matches);
            return matches;
        }

        protected static bool IsMatchingAliasedMethod(MethodInfo method, ControllerContext controllerContext, string actionName)
        {
            // return if aliased method is opting in to this request
            // to opt in, all attributes defined on the method must return true
            ReadOnlyCollection<ActionNameSelectorAttribute> attributes = ReflectedAttributeCache.GetActionNameSelectorAttributes(method);
            // Caching count is faster for ReadOnlyCollection
            int attributeCount = attributes.Count;
            // Performance sensitive, so avoid foreach
            for (int i = 0; i < attributeCount; i++)
            {
                if (!attributes[i].IsValidName(controllerContext, actionName, method))
                {
                    return false;
                }
            }
            return true;
        }

        protected static bool IsValidMethodSelector(ReadOnlyCollection<ActionMethodSelectorAttribute> attributes, ControllerContext controllerContext, MethodInfo method)
        {
            int attributeCount = attributes.Count;
            Contract.Assert(attributeCount > 0);
            for (int i = 0; i < attributeCount; i++)
            {
                if (!attributes[i].IsValidForRequest(controllerContext, method))
                {
                    return false;
                }
            }
            return true;
        }

        protected static void RunSelectionFilters(ControllerContext controllerContext, List<MethodInfo> methodInfos)
        {
            // Filter depending on the selection attribute.
            // Methods with valid selection attributes override all others.
            // Methods with one or more invalid selection attributes are removed.

            bool hasValidSelectionAttributes = false;
            // loop backwards for fastest removal
            for (int i = methodInfos.Count - 1; i >= 0; i--)
            {
                MethodInfo methodInfo = methodInfos[i];
                ReadOnlyCollection<ActionMethodSelectorAttribute> attrs = ReflectedAttributeCache.GetActionMethodSelectorAttributesCollection(methodInfo);
                if (attrs.Count == 0)
                {
                    // case 1: this method does not have a MethodSelectionAttribute

                    if (hasValidSelectionAttributes)
                    {
                        // if there is already method with a valid selection attribute, remove method without one
                        methodInfos.RemoveAt(i);
                    }
                }
                else if (IsValidMethodSelector(attrs, controllerContext, methodInfo))
                {
                    // case 2: this method has MethodSelectionAttributes that are all valid

                    // if a matching action method had a selection attribute, consider it more specific than a matching action method
                    // without a selection attribute
                    if (!hasValidSelectionAttributes)
                    {
                        // when the first selection attribute is discovered, remove any items later in the list without selection attributes
                        if (i + 1 < methodInfos.Count)
                        {
                            methodInfos.RemoveFrom(i + 1);
                        }
                        hasValidSelectionAttributes = true;
                    }
                }
                else
                {
                    // case 3: this method has a method selection attribute but it is not valid

                    // remove the method since it is opting out of this request
                    methodInfos.RemoveAt(i);
                }
            }
        }

        // Get the action name for the method.         
        public string GetActionName(MethodInfo methodInfo)
        {
            // Check for ActionName attribute
            object[] nameAttributes = methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), inherit: true);
            if (nameAttributes.Length > 0)
            {
                ActionNameAttribute nameAttribute = nameAttributes[0] as ActionNameAttribute;
                if (nameAttribute != null)
                {
                    return nameAttribute.Name;
                }
            }

            return GetCanonicalMethodName(methodInfo);
        }
        
        public MethodInfo FindActionMethod(ControllerContext controllerContext, string actionName)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            RouteData routeData = controllerContext.RouteData;

            if (routeData != null)
            {
                MethodInfo target = routeData.GetTargetActionMethod();
                if (target != null)
                {
                    // short circuit the selection process if we matched a direct route that already supplies the method. 
                    return target;
                }
            }

            // If this controller has a Route attribute, then its actions can't be reached by standard routes. 
            if (_hasRouteAttributeOnController)
            {
                if (routeData == null)
                {
                    return null;
                }
                ControllerDescriptor descriptor = routeData.GetTargetControllerDescriptor();

                if (descriptor == null)
                {
                    // Missing descriptor. It's a standard route. Can't reach actions. 
                    return null;
                }
            }

            List<MethodInfo> finalMethods = FindActionMethods(controllerContext, actionName);

            switch (finalMethods.Count)
            {
                case 0:
                    return null;

                case 1:
                    return finalMethods[0];

                default:
                    throw CreateAmbiguousActionMatchException(finalMethods, actionName);
            }
        }
    }
}