// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a controller-level attribute that can be used to enable OData action selection based on routing conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ODataRoutingAttribute : Attribute
    {
        // This class is not needed; Routing is injected in ODataServiceCollectionExtensions::AddOdata()
    }
}