using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ODataSample.Web.Models
{
	public class ApplicationUser : IdentityUser<string>
	{
		public int? UsedProductId { get; set; }
		public Product UsedProduct { get; set; }
		public Product FavouriteProduct { get; set; }
		public int? FavouriteProductId { get; set; }
		public List<Product> ProductsCreated { get; set; }
		public List<Product> ProductsLastModified { get; set; }
	}
}