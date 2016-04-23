using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OData;

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

		[Required]
		[RegularExpression("[^0-9]+", ErrorMessage = "The name cannot contain any numbers")]
		public string Name { get; set; }
		[Required]
		[DisplayName("export price")]
		[Range((double)5, 10)]
		public double Price { get; set; }
		[Required(ErrorMessage = "Please enter an email address")]
		[EmailAddress(ErrorMessage = "Please provide a valid email address")]
		public string OwnerEmailAddress { get; set; }
		public DateTimeOffset DateInvented { get; set; }
		public DateTimeOffset DateCreated { get; set; }
		public string SomeSecretFieldThatShouldNotBeReturned { get; set; }
	}
}
