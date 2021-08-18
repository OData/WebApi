//-----------------------------------------------------------------------------
// <copyright file="Address.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class Address
    {
        public int HouseNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public ZipCode ZipCode { get; set; }
        public string IgnoreThis { get; set; }
    }
}
