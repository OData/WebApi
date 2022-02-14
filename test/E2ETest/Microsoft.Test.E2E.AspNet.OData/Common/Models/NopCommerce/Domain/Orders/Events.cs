//-----------------------------------------------------------------------------
// <copyright file="Events.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Orders
{
    public class OrderPaidEvent
    {
        private readonly Order _order;

        public OrderPaidEvent(Order order)
        {
            this._order = order;
        }

        public Order Order
        {
            get { return _order; }
        }
    }

    public class OrderPlacedEvent
    {
        private readonly Order _order;

        public OrderPlacedEvent(Order order)
        {
            this._order = order;
        }

        public Order Order
        {
            get { return _order; }
        }
    }
}
