// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace System.Web.Mvc.Filters
{
    public interface IOverrideFilter
    {
        Type FiltersToOverride { get; }
    }
}
