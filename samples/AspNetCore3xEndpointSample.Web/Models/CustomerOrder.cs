//-----------------------------------------------------------------------------
// <copyright file="CustomerOrder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore3xEndpointSample.Web.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual Address HomeAddress { get; set; }

        public virtual IList<Address> FavoriteAddresses { get; set; }

        public virtual Order Order { get; set; }

        public virtual IList<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }

    [Owned, ComplexType]
    public class Address
    {
        public string City { get; set; }

        public string Street { get; set; }
    }
}
