// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter
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
