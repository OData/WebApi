// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore3xEndpointSample.Web.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            if (_edmModel == null)
            {
                var builder = new ODataConventionModelBuilder();
                var customers = builder.EntitySet<Customer>("Customers");
                customers.Binding.HasManyPath(c => c.CustomerReferrals, true).HasRequiredBinding(r => r.ReferredCustomer, "Customers");
                //     builder.EntitySet<Order>("Orders");
                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

    }

    public class Customer
    {
        [Key]
        public int ID { get; set; }

        [Contained]
        public virtual ICollection<CustomerReferral> CustomerReferrals { get; set; }

        [Contained]
        public virtual ICollection<CustomerPhone> Phones { get; set; }
    }

    public class CustomerReferral
    {
        [Key]
        public int ID { get; set; }

        public int CustomerID { get; set; }

        public int ReferredCustomerID { get; set; }

        [Required]
        [ForeignKey(nameof(CustomerID))]
        public virtual Customer Customer { get; set; }

        [Required]
        [ForeignKey(nameof(ReferredCustomerID))]
        public virtual Customer ReferredCustomer { get; set; }
    }

    public class CustomerPhone
    {
        [Key]
        public int ID { get; set; }

        [Editable(false)]
        public int CustomerID { get; set; }

        [Contained]
        public virtual CustomerPhoneNumberFormatted Formatted { get; set; }
    }

    public class CustomerPhoneNumberFormatted
    {
        [Key]
        public int CustomerPhoneNumberID { get; set; }

        public string FormattedNumber { get; set; }
    }
}