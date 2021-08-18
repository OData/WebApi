//-----------------------------------------------------------------------------
// <copyright file="ZipCode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
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
