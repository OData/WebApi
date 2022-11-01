//-----------------------------------------------------------------------------
// <copyright file="OrderLine.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class OrderTest
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int OrderId { get; set; }

        [Contained]
        public IList<OrderLineDetail> OrderLineDetails { get; set; }
    }
}
