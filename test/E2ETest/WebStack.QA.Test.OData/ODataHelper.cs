// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace WebStack.QA.Test.OData
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
