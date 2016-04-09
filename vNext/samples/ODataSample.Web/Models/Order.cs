namespace ODataSample.Web.Models
{
	public class Order
	{
		public string Id { get; set; }
		public string Title { get;set; }
		public int CustomerId { get; set; }
		public Customer Customer { get; set; }
	}
}