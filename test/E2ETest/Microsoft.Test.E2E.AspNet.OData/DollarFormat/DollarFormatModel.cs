//-----------------------------------------------------------------------------
// <copyright file="DollarFormatModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.DollarFormat
{
    public class DollarFormatCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<DollarFormatOrder> Orders { get; set; }
        public DollarFormatOrder SpecialOrder { get; set; }
    }

    public class DollarFormatOrder
    {
        public int Id { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public string Detail { get; set; }
    }
}
