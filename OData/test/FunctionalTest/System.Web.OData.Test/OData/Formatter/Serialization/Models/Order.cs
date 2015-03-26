// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Formatter.Serialization.Models
{
    public class Order
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Customer Customer { get; set; }
        public IDictionary<string, object> OrderProperties { get; set; }
    }
}
