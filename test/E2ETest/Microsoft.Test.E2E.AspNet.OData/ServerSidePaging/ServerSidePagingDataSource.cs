//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.ServerSidePaging
{
    public static class ContainmentPagingDataSource
    {
        internal const int TargetSize = 3;

        private static readonly List<ContainedPagingOrderItem> orderItems = new List<ContainedPagingOrderItem>(
            Enumerable.Range(1, TargetSize * TargetSize * TargetSize).Select(idx => new ContainedPagingOrderItem
            {
                Id = idx
            }));

        private static readonly List<ContainedPagingOrder> orders = new List<ContainedPagingOrder>(
            Enumerable.Range(1, TargetSize * TargetSize).Select(idx => new ContainedPagingOrder
            {
                Id = idx,
                Items = orderItems.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<ContainmentPagingCustomer> customers = new List<ContainmentPagingCustomer>(
            Enumerable.Range(1, TargetSize).Select(idx => new ContainmentPagingCustomer
            {
                Id = idx,
                Orders = orders.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<ContainedPagingNote> notes = new List<ContainedPagingNote>(
            Enumerable.Range(1, TargetSize * TargetSize * TargetSize * TargetSize).Select(idx => new ContainedPagingNote
            {
                Id = idx
            }));

        private static readonly List<ContainedPagingItem> items = new List<ContainedPagingItem>(
            Enumerable.Range(1, TargetSize * TargetSize * TargetSize).Select(idx => new ContainedPagingExtendedItem
            {
                Id = idx,
                Notes = notes.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<ContainedPagingTab> tabs = new List<ContainedPagingTab>(
            Enumerable.Range(1, TargetSize * TargetSize).Select(idx => new ContainedPagingExtendedTab
            {
                Id = idx,
                Items = items.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<ContainmentPagingPanel> panels = new List<ContainmentPagingPanel>(
            Enumerable.Range(1, TargetSize * TargetSize).Select(idx => new ContainmentPagingExtendedPanel
            {
                Id = idx,
                Items = items.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<ContainmentPagingMenu> menus = new List<ContainmentPagingMenu>(
            Enumerable.Range(1, TargetSize).Select(idx => new ContainmentPagingExtendedMenu
            {
                Id = idx,
                Tabs = tabs.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList(),
                Panels = panels.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        public static List<ContainmentPagingCustomer> Customers => customers;

        public static List<ContainedPagingOrder> Orders => orders;

        public static List<ContainmentPagingMenu> Menus => menus;

        public static List<ContainedPagingTab> Tabs => tabs;
    }

    public static class NoContainmentPagingDataSource
    {
        private const int TargetSize = 3;

        private static readonly List<NoContainmentPagingOrderItem> orderItems = new List<NoContainmentPagingOrderItem>(
            Enumerable.Range(1, TargetSize * TargetSize * TargetSize).Select(idx => new NoContainmentPagingOrderItem
            {
                Id = idx
            }));

        private static readonly List<NoContainmentPagingOrder> orders = new List<NoContainmentPagingOrder>(
            Enumerable.Range(1, TargetSize * TargetSize).Select(idx => new NoContainmentPagingOrder
            {
                Id = idx,
                Items = orderItems.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        private static readonly List<NoContainmentPagingCustomer> customers = new List<NoContainmentPagingCustomer>(
            Enumerable.Range(1, TargetSize).Select(idx => new NoContainmentPagingCustomer
            {
                Id = idx,
                Orders = orders.Skip((idx - 1) * TargetSize).Take(TargetSize).ToList()
            }));

        public static List<NoContainmentPagingCustomer> Customers => customers;

        public static List<NoContainmentPagingOrder> Orders => orders;
    }
}
