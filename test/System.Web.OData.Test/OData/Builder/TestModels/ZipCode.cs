// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder.TestModels
{
    public class ZipCode
    {
        public string Part1 { get; set; }
        public string Part2 { get; set; }
    }

    public class RecursiveZipCode
    {
        public string Part1 { get; set; }
        public string Part2 { get; set; }
        public RecursiveZipCode Recursive { get; set; }
    }
}
