// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
}
