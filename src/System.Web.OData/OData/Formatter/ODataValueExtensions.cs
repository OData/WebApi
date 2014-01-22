// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;

namespace System.Web.OData.Formatter
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
