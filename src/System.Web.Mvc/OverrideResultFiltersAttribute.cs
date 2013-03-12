// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class OverrideResultFiltersAttribute : Attribute, IOverrideFilter
    {
        public Type FiltersToOverride
        {
            get { return typeof(IResultFilter); }
        }
    }
}
