using System;
using ODataSample.Web.Controllers;

namespace ODataSample.Web.Models
{
	public class Seeder
	{
		public static void EnsureDatabase(ApplicationDbContext context)
		{
			//context.Database.EnsureCreated();
			// Add Mvc.Client to the known applications.
			var productsCrud = new CrudBase<Product, int>(
				context, context.Products, p => p.ProductId);
			var customersCrud = new CrudBase<Customer, int>(
				context, context.Customers, p => p.CustomerId);

			var productId = 0;
			Action<string, double, int?, DateTime?> prod = (name, price, customerId, dateCreated) =>
			{
				productsCrud.EnsureEntity(
					++productId, product =>
					{
						product.Name = name;
						product.Price = price;
						product.CustomerId = customerId;
						product.DateCreated = dateCreated;
					});
			};
			var currentCustomerId = 0;
			Action<string, string> cust = (firstName, lastName) =>
			{
				customersCrud.EnsureEntity(
					++currentCustomerId, customer =>
					{
						customer.FirstName = firstName;
						customer.LastName = lastName;
					});
			};

			cust("Harry", "Whitburn");
			cust("Nick", "Lawden");
			context.SaveChanges();
			prod("Apple number1", 10, null, null);
			prod("Apple number1", 10, 1, null);
			prod("Orange number1", 20, null, new DateTime(2015, 12, 1));
			prod("Peanut butter number1", 25, 2, null);
			prod("xApple number2", 10, 1, null);
			prod("xOrange number2", 20, 2, null);
			prod("xPeanut butter number2", 25, 2, null);
			prod("xApple number2", 10, 1, null);
			prod("xOrange number2", 20, 2, null);
			prod("xPeanut butter number2", 25, 2, null);
			prod("xApple number2", 10, 1, null);
			prod("xOrange number2", 20, 2, null);
			prod("xPeanut butter number2", 25, 2, null);
			prod("xApple number2", 10, 1, null);
			prod("xOrange number2", 20, 2, null);
			prod("xPeanut butter number2", 25, 2, null);
			prod("Apple number3", 10, 1, null);
			prod("Orange number3", 20, 2, null);
			prod("Peanut butter number3", 25, 2, null);
			prod("Apple number4", 10, 1, null);
			prod("Orange number4", 20, 2, null);
			prod("Peanut butter number4", 25, 2, null);
			prod("Apple number5", 10, 1, null);
			prod("Orange number5", 20, 2, null);
			prod("Peanut butter number5", 25, 2, null);
			prod("Apple number6", 10, 1, null);
			prod("Orange number6", 20, 2, null);
			prod("Peanut butter number6", 25, 2, null);
			context.SaveChanges();
		}
	}
}