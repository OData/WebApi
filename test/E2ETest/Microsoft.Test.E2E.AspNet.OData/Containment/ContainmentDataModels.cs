//-----------------------------------------------------------------------------
// <copyright file="ContainmentDataModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.Containment
{
    public class Account
    {
        public int AccountID { get; set; }
        public string Name { get; set; }
        [Contained]
        public List<PaymentInstrument> PayinPIs { get; set; }
        [Contained]
        public PaymentInstrument PayoutPI { get; set; }
    }

    public class PremiumAccount : Account
    {
        [Contained]
        [Required]
        public GiftCard GiftCard { get; set; }
    }

    public class GiftCard
    {
        public int GiftCardID { get; set; }
        public string GiftCardNO { get; set; }
        public double Amount { get; set; }
    }

    public class PaymentInstrument
    {
        public int PaymentInstrumentID { get; set; }
        public string FriendlyName { get; set; }
        [Contained]
        public Statement Statement { get; set; }
    }

    public class Statement
    {
        public int StatementID { get; set; }
        public string TransactionDescription { get; set; }
        public double Amount { get; set; }
    }
}
