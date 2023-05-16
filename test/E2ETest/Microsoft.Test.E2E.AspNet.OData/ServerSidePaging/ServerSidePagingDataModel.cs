//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.ServerSidePaging
{
    public class ServerSidePagingCustomer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<ServerSidePagingOrder> ServerSidePagingOrders { get; set; }
    }

    public class ServerSidePagingOrder
    {
        [Key]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public ServerSidePagingCustomer ServerSidePagingCustomer { get; set; }
    }

    public class ServerSidePagingEmployee
    {
        public int Id { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class SkipTokenPagingCustomer
    {
        public int Id { get; set; }
        public string Grade { get; set; }
        public decimal? CreditLimit { get; set; }
        public DateTime? CustomerSince { get; set; }
    }

    public class SkipTokenPagingEdgeCase1Customer
    {
        public int Id { get; set; }
        public decimal? CreditLimit { get; set; }
    }

    public class ContainmentPagingCustomer
    {
        public int Id { get; set; }
        [Contained]
        public List<ContainedPagingOrder> Orders { get; set; }
    }

    public class ContainedPagingOrder
    {
        public int Id { get; set; }
        [Contained]
        public List<ContainedPagingOrderItem> Items { get; set; }
    }

    public class ContainedPagingOrderItem
    {
        public int Id { get; set; }
    }

    public class NoContainmentPagingCustomer
    {
        public int Id { get; set; }
        public List<NoContainmentPagingOrder> Orders { get; set; }
    }

    public class NoContainmentPagingOrder
    {
        public int Id { get; set; }
        public List<NoContainmentPagingOrderItem> Items { get; set; }
    }

    public class NoContainmentPagingOrderItem
    {
        public int Id { get; set; }
    }

    public class ContainmentPagingMenu
    {
        public int Id { get; set; }
    }

    public class ContainmentPagingExtendedMenu : ContainmentPagingMenu
    {
        [Contained]
        public List<ContainedPagingTab> Tabs { get; set; }
        // Non-contained
        public List<ContainmentPagingPanel> Panels { get; set; }
    }

    public class ContainedPagingTab
    {
        public int Id { get; set; }
    }

    public class ContainedPagingExtendedTab : ContainedPagingTab
    {
        [Contained]
        public List<ContainedPagingItem> Items { get; set; }
    }

    public class ContainedPagingItem
    {
        public int Id { get; set; }
    }

    public class ContainedPagingExtendedItem : ContainedPagingItem
    {
        [Contained]
        public List<ContainedPagingNote> Notes { get; set; }
    }

    public class ContainedPagingNote
    {
        public int Id { get; set; }
    }

    public class ContainmentPagingPanel
    {
        public int Id { get; set; }
    }

    public class ContainmentPagingExtendedPanel : ContainmentPagingPanel
    {
        [Contained]
        public List<ContainedPagingItem> Items { get; set; }
    }
}
