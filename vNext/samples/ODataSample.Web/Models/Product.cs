using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataSample.Web.Models
{
	public class Product
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ProductId { get; set; }
		public Customer Customer { get; set; }
		public int? CustomerId { get; set; }
		public string CreatedByUserId { get; set; }
		public ApplicationUser CreatedByUser { get; set; }

		public string LastModifiedByUserId { get; set; }
		public ApplicationUser LastModifiedByUser { get; set; }

		public string Name { get; set; }
        public double Price { get; set; }
		public DateTime? DateCreated { get; set; }
		public string SomeSecretFieldThatShouldNotBeReturned { get; set; }
    }
}
