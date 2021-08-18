//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
