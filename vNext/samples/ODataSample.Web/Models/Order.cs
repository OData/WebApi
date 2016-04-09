using System;

namespace ODataSample.Web.Models
{
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