//-----------------------------------------------------------------------------
// <copyright file="Client.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
