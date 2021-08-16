//-----------------------------------------------------------------------------
// <copyright file="ODataQueryParameterBindingAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
                parameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
            {
                return parameterType.GetGenericArguments().Single();
            }

            return null;
        }
    }
}
