using System.Collections.Generic;

namespace ODataSample.Web.Models
{
	public class Customer : DbObject
	{
		public int CustomerId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<Product> Products { get; set; }
		public List<Order> Orders { get; set; }
	}
}
