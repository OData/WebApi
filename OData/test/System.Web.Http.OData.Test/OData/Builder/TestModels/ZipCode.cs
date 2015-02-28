// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.TestModels
{
    public class ZipCode
    {
        public string Part1 { get; set; }
        public string Part2 { get; set; }
        public ZipCode Recursive { get; set; }
    }
}
