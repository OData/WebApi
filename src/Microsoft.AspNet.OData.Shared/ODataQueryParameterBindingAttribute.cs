// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// A ParameterBindingAttribute to bind parameters of type ODataQueryOptions to the OData query from the incoming request.
    /// </summary>
    public partial class ODataQueryParameterBindingAttribute
    {
        internal static Type GetEntityClrTypeFromParameterType(Type parameterType)
        {
            Contract.Assert(parameterType != null);

            if (parameterType.IsGenericType &&
                (parameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>) ||
                 parameterType.GetGenericTypeDefinition() == typeof(IODataQueryOptions<>)))
            {
                return parameterType.GetGenericArguments().Single();
            }

            return null;
        }
    }
}
