//-----------------------------------------------------------------------------
// <copyright file="PhoneNumber.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.Common.Models
{
    public struct PhoneNumber
    {
        public int CountryCode { get; set; }

        public int AreaCode { get; set; }

        public int Number { get; set; }

        public PhoneType PhoneType { get; set; }
    }
}
