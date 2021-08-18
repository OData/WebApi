//-----------------------------------------------------------------------------
// <copyright file="IsOfModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public class BillingCustomer
    {
        [Key]
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Birthday { get; set; }

        public Guid? Token { get; set; }

        public CustomerType CustomerType { get; set; }

        public BillingAddress Address { get; set; }

        public BillingDetail Billing { get; set; }
    }

    public class BillingDetail
    {
        public int Id { get; set; }

        public string Owner { get; set; }
    }

    public class CreditCard : BillingDetail
    {
        public CardType CardType { get; set; }

        public int ExpiryYear { get; set; }
    }

    public class BankAccount : BillingDetail
    {
        public string BankName { get; set; }
    }

    [ComplexType]
    public class BillingAddress
    {
        public string City { get; set; }
    }

    public class BillingCnAddress : BillingAddress
    {
        public int PostCode { get; set; }
    }

    public class BillingUsAddress : BillingAddress
    {
        public string Street { get; set; }
    }

    public enum CardType
    {
        ZeroOrLowInterestRate,

        Rewards,

        Secured,

        Student
    }

    public enum CustomerType
    {
        Normal,

        Vip
    }
}
