// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class Client
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<MyOrder> MyOrders { get; set; }
    }
}
