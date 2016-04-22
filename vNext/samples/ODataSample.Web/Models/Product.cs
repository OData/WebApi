using System;
using System.Collections.Generic;

namespace ODataSample.Web.Models
{
	public class ProductBase : DbObject
	{
		public int ProductId { get; set; }
	}
	public class Product : ProductBase
	{
		public Customer Customer { get; set; }
		public int? CustomerId { get; set; }
		public string CreatedByUserId { get; set; }
		public ApplicationUser CreatedByUser { get; set; }
		public List<ApplicationUser> UsedByUsers { get; set; }


		public string LastModifiedByUserId { get; set; }
		public ApplicationUser LastModifiedByUser { get; set; }

		public string Name { get; set; }
		public double Price { get; set; }
		public DateTimeOffset DateInvented { get; set; }
		public DateTimeOffset DateCreated { get; set; }
		public string SomeSecretFieldThatShouldNotBeReturned { get; set; }
	}
}
