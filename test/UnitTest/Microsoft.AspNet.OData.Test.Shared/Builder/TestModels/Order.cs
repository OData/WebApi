﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
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
