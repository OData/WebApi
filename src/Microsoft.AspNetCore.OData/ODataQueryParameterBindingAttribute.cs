// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A <see cref="ParameterBindingAttribute"/> to bind parameters of type <see cref="ODataQueryOptions"/> to the OData query from the incoming request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed partial class ODataQueryParameterBindingAttribute : Attribute
    {
    }
}
