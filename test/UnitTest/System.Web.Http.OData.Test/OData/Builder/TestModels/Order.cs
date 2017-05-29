// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.TestModels
{
    public class Order
    {
        public int OrderId { get; set; }
        public Customer Customer { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
