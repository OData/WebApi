using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataSample.Web.Models
{
    public class Product
    {
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ProductId { get; set; }
		public int CustomerId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
		public DateTime? DateCreated { get; set; }
    }
}
