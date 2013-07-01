// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc.Properties;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace System.Web.Mvc
{
    internal sealed class ActionMethodSelector
    {
        public ActionMethodSelector(Type controllerType)
        {
            ControllerType = controllerType;
            PopulateLookupTables();
        }

        public Type ControllerType { get; private set; }

        public MethodInfo[] AliasedMethods { get; private set; }

        public ILookup<string, MethodInfo> NonAliasedMethods { get; private set; }

        private AmbiguousMatchException CreateAmbiguousMatchException(List<MethodInfo> ambiguousMethods, string actionName)
        {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods)
            {
                string controllerAction = Convert.ToString(methodInfo, CultureInfo.CurrentCulture);
                string controllerType = methodInfo.DeclaringType.FullName;
                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, controllerAction, controllerType);
            }
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                                           actionName, ControllerType.Name, exceptionMessageBuilder);
            return new AmbiguousMatchException(message);
        }

        public MethodInfo FindActionMethod(ControllerContext controllerContext, string actionName)
        {
            Contract.Assert(controllerContext != null);

            if (controllerContext.RouteData != null)
            {
                MethodInfo target = controllerContext.RouteData.GetTargetActionMethod();
                if (target != null)
                {
                    // short circuit the selection process if a direct route was matched.
                    return target;
                }
            }

            List<MethodInfo> finalMethods = FindActionMethods(controllerContext, actionName, AliasedMethods, NonAliasedMethods);

            switch (finalMethods.Count)
            {
                case 0:
                    return null;

                case 1:
                    return finalMethods[0];

                default:
                    throw CreateAmbiguousMatchException(finalMethods, actionName);
            }  
        }
        
        internal static List<MethodInfo> FindActionMethods(ControllerContext controllerContext, string actionName, MethodInfo[] aliasedMethods, ILookup<string, MethodInfo> nonAliasedMethods)
        {
            List<MethodInfo> matches = new List<MethodInfo>();

            // Performance sensitive, so avoid foreach
            for (int i = 0; i < aliasedMethods.Length; i++)
            {
                MethodInfo method = aliasedMethods[i];
                if (IsMatchingAliasedMethod(method, controllerContext, actionName))
                {
                    matches.Add(method);
                }
            }
            matches.AddRange(nonAliasedMethods[actionName]);
            RunSelectionFilters(controllerContext, matches);
            return matches;
        }

        private static bool IsMatchingAliasedMethod(MethodInfo method, ControllerContext controllerContext, string actionName)
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

        private static bool IsValidMethodSelector(ReadOnlyCollection<ActionMethodSelectorAttribute> attributes, ControllerContext controllerContext, MethodInfo method)
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
        
        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        private static bool IsValidActionMethod(MethodInfo methodInfo)
        {
            return !(methodInfo.IsSpecialName ||
                     methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
        }

        private void PopulateLookupTables()
        {
            MethodInfo[] allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            AliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            NonAliasedMethods = actionMethods.Except(AliasedMethods).ToLookup(method => method.Name, StringComparer.OrdinalIgnoreCase);
        }

        private static void RunSelectionFilters(ControllerContext controllerContext, List<MethodInfo> methodInfos)
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
    }
}
