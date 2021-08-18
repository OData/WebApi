//-----------------------------------------------------------------------------
// <copyright file="ODataValueExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    internal static class ODataValueExtensions
    {
        public static object GetInnerValue(this ODataValue odataValue)
        {
            if (odataValue is ODataNullValue)
            {
                return null;
            }

            ODataPrimitiveValue oDataPrimitiveValue = odataValue as ODataPrimitiveValue;
            if (oDataPrimitiveValue != null)
            {
                return oDataPrimitiveValue.Value;
            }

            return odataValue;
        }
    }
}
