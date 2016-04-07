using System;

namespace ODataSample.Web.Models
{
	public interface IProduct
	{
		int CustomerId { get; set; }
		DateTime? DateCreated { get; set; }
		string Name { get; set; }
		double Price { get; set; }
		int ProductId { get; set; }
	}
}