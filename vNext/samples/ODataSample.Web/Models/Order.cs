using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ODataSample.Web.Models
{
	public class ApplicationRole : IdentityRole<string>
	{
		
	}
	//public class ApplicationUserRole : IdentityUserRole<string>
	//{
	//	// Hack for now
	//	public string Id { get; set; }
	//	public override string UserId { get; set; }
	//	public override string RoleId { get; set; }
	//}
	public class Order : Order<string>
	{
		public string Title { get; set; }
		public int CustomerId { get; set; }
		public Customer Customer { get; set; }
	}

	public class Order<T>
	{
		public T Id { get; set; }
	}
}