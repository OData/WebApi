using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataSample.Web.Models
{
    public class Customer
    {
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Product> Products { get; set; }
    }
}
