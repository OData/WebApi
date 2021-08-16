//-----------------------------------------------------------------------------
// <copyright file="DollarQueryModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.DollarQuery
{
    public class DollarQueryCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<DollarQueryOrder> Orders { get; set; }
        public DollarQueryOrder SpecialOrder { get; set; }
    }

    public class DollarQueryOrder
    {
        public int Id { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public string Detail { get; set; }
    }
}
