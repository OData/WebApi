//-----------------------------------------------------------------------------
// <copyright file="ODataHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData
{
    public static class ODataHelper
    {
        public static string GetHttpPrefix(string httpMethod)
        {
            switch (httpMethod)
            {
                case "GET":
                    return "Get";
                case "POST":
                    return "Post";
                case "PUT":
                    return "Put";
                case "MERGE":
                case "PATCH":
                    return "Patch";
                case "DELETE":
                    return "Delete";
                default:
                    return null;
            }
        }

        public static void ThrowAtomNotSupported()
        {
            throw new InvalidOperationException("MUST FIXED: ATOM is disable in V4, we should not test it in positive case.");
        }
    }
}
