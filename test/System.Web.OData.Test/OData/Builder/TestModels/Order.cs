// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder.TestModels
{
    public class Order
    {
        public int OrderId { get; set; }
        public Customer Customer { get; set; }
        public Decimal Cost { get; set; }
        public Decimal Price { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public DateTimeOffset? DeliveryDate { get; set; }
    }
}
