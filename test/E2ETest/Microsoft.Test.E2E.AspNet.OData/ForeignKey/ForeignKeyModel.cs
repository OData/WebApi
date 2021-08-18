//-----------------------------------------------------------------------------
// <copyright file="ForeignKeyModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace Microsoft.Test.E2E.AspNet.OData.ForeignKey
{
    public class ForeignKeyCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<ForeignKeyOrder> Orders { get; set; }
    }

    public class ForeignKeyOrder
    {
        [Key]
        public int OrderId { get; set; }

        public string OrderName { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        [ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public ForeignKeyCustomer Customer { get; set; }
    }
}
