// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Builder.TestModels
{
    public class Client
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<MyOrder> MyOrders { get; set; }
    }
}
