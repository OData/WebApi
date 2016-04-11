using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Controllers;

namespace ODataSample.Web.Models
{
	public class Seeder
	{
		public IServiceProvider ServiceProvider { get; set; }

		public static async Task SeedAllAsync(IServiceProvider serviceProvider)
		{
			await new Seeder(serviceProvider)
				.EnsureDatabaseAsync();
		}

		private CrudBase<Product, int> _productsCrud;
		private int _productId;
		private CrudBase<Order, string> _ordersCrud;
		private UserManager<ApplicationUser> _userManager;

		public Seeder(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public async Task EnsureDatabaseAsync()
		{
			MigrateDatabase(ServiceProvider);
			_userManager = ServiceProvider.GetService<UserManager<ApplicationUser>>();
			var roleManager = ServiceProvider.GetService<RoleManager<IdentityRole>>();
			await EnsureUserAsync("1", "testy@example.com", "testy@example.com", 7);
			await EnsureUserAsync("2", "testy@testy.com", "testy@testy.com", 12);
			await EnsureUserAsync("3", "money@boy.com", "money@boy.com", 13);
			using (var context = ServiceProvider.GetRequiredService<ApplicationDbContext>())
			{
				await roleManager.CreateAsync(new IdentityRole("Admin"));
				await roleManager.CreateAsync(new IdentityRole("User"));
				await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("1"), "Admin");
				await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("1"), "User");
				await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("2"), "User");
				//var userStore = new UserStore<ApplicationUser>(context);
				////var roleManager = new RoleManager<IdentityRole>();
				//var userManager = new UserManager<ApplicationUser>(
				//	, );
				context.Database.EnsureCreated();
				// Add Mvc.Client to the known applications.
				_productsCrud = new CrudBase<Product, int>(
					context, context.Products, p => p.ProductId);
				_ordersCrud = new CrudBase<Order, string>(
					context, context.Orders, p => p.Id);
				var customersCrud = new CrudBase<Customer, int>(
					context, context.Customers, p => p.CustomerId);

				//Action<string, double, int?, DateTime?, string> prod = (name, price, customerId, dateCreated, cratedByUserId) =>
				//{
				//	productId = Prod(productsCrud, productId, name, price, customerId, dateCreated, cratedByUserId);
				//};
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
				cust("Emil", "Roijer");
				context.SaveChanges();
				Prod("Apple number1", 10, null, null);
				Prod("Apple number1", 10, 1, null, "1");
				Prod("Orange number1", 20, null, new DateTime(2015, 12, 1));
				Prod("Peanut butter number1", 25, 2, null);
				Prod("xApple number2", 10, 1, null);
				Prod("xOrange number2", 20, 2, null);
				Prod("xPeanut butter number2", 25, 2, null);
				Prod("xApple number2", 10, 1, null);
				Prod("xOrange number2", 20, 2, null);
				Prod("xPeanut butter number2", 25, 2, null);
				Prod("xApple number2", 10, 1, null);
				Prod("xOrange number2", 20, 2, null);
				Prod("xPeanut butter number2", 25, 2, null);
				Prod("xApple number2", 10, 1, null);
				Prod("xOrange number2", 20, 2, null);
				Prod("xPeanut butter number2", 25, 2, null);
				Prod("Apple number3", 10, 1, null);
				Prod("Orange number3", 20, 2, null);
				Prod("Peanut butter number3", 25, 2, null);
				Prod("Apple number4", 10, 1, null);
				Prod("Orange number4", 20, 2, null);
				Prod("Peanut butter number4", 25, 2, null);
				Prod("Apple number5", 10, 1, null);
				Prod("Orange number5", 20, 2, null);
				Prod("Peanut butter number5", 25, 2, null);
				Prod("Apple number6", 10, 1, null);
				Prod("Orange number6", 20, 2, null);
				Prod("Peanut butter number6", 25, 2, null);
				context.SaveChanges();
				Order("1", "First order", 1);
				Order("2", "Second order", 2);
				Order("3", "Third order", 1);
				context.SaveChanges();
			}
		}

		private async Task EnsureUserAsync(string id, string userName, string email, int? favouriteProductId)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				user = new ApplicationUser
				{
					Id = id,
					UserName = userName,
					Email = email,
					FavouriteProductId = favouriteProductId
				};
				await _userManager.CreateAsync(user);
			}
			else
			{
				user.UserName = userName;
				user.Email = email;
				user.FavouriteProductId = favouriteProductId;
				await _userManager.UpdateAsync(user);
			}
		}

		private int Prod(string name, double price, int? customerId,
			DateTime? dateCreated, string cratedByUserId = null)
		{
			_productsCrud.EnsureEntity(
				++_productId, product =>
				{
					product.Name = name;
					product.Price = price;
					product.CustomerId = customerId;
					product.DateCreated = dateCreated;
					product.CreatedByUserId = cratedByUserId;
				});
			return _productId;
		}

		private void Order(string id, string title, int customerId)
		{
			_ordersCrud.EnsureEntity(
				id, entity =>
				{
					entity.Id = id;
					entity.Title = title;
					entity.CustomerId = customerId;
				});
		}

		public static void MigrateDatabase(IServiceProvider serviceProvider)
		{
			var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
			if (context != null)
			{
				context.Database.Migrate();
			}
			else
			{
				throw new Exception("Unable to resolve database context");
			}
		}
	}
}