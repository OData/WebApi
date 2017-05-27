using System;
using System.Data.SqlClient;
using System.Linq;
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
		private CrudBase<Order, int> _ordersCrud;
		private UserManager<ApplicationUser> _userManager;
		public ApplicationDbContext Context { get; private set; }

		public Seeder(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public async Task EnsureDatabaseAsync()
		{
			await MigrateDatabaseAsync(ServiceProvider);
			_userManager = ServiceProvider.GetService<UserManager<ApplicationUser>>();
			var roleManager = ServiceProvider.GetService<RoleManager<IdentityRole>>();
			await EnsureUsersAsync(false);
			Context = ServiceProvider.GetRequiredService<ApplicationDbContext>();
			//var userStore = new UserStore<ApplicationUser>(context);
			////var roleManager = new RoleManager<IdentityRole>();
			//var userManager = new UserManager<ApplicationUser>(
			//	, );
			EnsureDatabaseExists();
			Context.Database.EnsureCreated();
			// Add Mvc.Client to the known applications.
			_productsCrud = new CrudBase<Product, int>(
				Context, Context.Products, p => p.ProductId);
			_ordersCrud = new CrudBase<Order, int>(
				Context, Context.Orders, p => p.Id);
			var customersCrud = new CrudBase<Customer, int>(
				Context, Context.Customers, p => p.CustomerId);

			//Action<string, double, int?, DateTime?, string> prod = (name, price, customerId, dateCreated, cratedByUserId) =>
			//{
			//	productId = Prod(productsCrud, productId, name, price, customerId, dateCreated, cratedByUserId);
			//};
			Action<string, string, string> cust = (guid, firstName, lastName) =>
			{
				customersCrud.EnsureEntity(
					guid, customer =>
					{
						customer.FirstName = firstName;
						customer.LastName = lastName;
					});
			};

			cust("6c13ec25-b9cf-4c99-87e7-13a45a034342", "Harry", "Whitburn");
			cust("0e941f32-8ae1-4eae-875a-f748bae3ce2a", "Nick", "Lawden");
			cust("134bd420-f64a-40db-bbe4-0b9a9ff3eb01", "Emil", "Roijer");
			Context.SaveChanges();
			Prod("00d5c0b0-738d-41ff-86d4-dca84da1d4d5", "Apple number1", 10, null, null);
			Prod("cd4c554d-3ca2-49aa-9286-08abacba3fa0", "Apple number1", 10, 1, null, "1");
			Prod("d100c753-3335-439c-88db-b8d9b6c09cc8", "Orange number1", 20, null, null);
			Prod("da93b8a4-4378-4ce0-918e-653f1bd4acaf", "Peanut butter number1", 25, 2, null);
			Prod("fe208521-4a77-4b09-94c2-2f08a4dd8872", "xApple number2", 10, 1, null);
			Prod("844fbc16-3d77-4421-9498-aa65300bc789", "xOrange number2", 20, 2, null);
			Prod("78509968-0073-4c57-8774-ec0d58ea7ff3", "xPeanut butter number2", 25, 2, null);
			Prod("18b8c4e7-48a2-40c7-bea8-1d1879bf852e", "xApple number2", 10, 1, null);
			Prod("e5ab744c-441c-447b-ae92-055b1bce5541", "xOrange number2", 20, 2, null);
			Prod("3a5476b1-7dd8-40b0-a6ce-b6bd3127da6f", "xPeanut butter number2", 25, 2, null);
			Prod("633a125c-1fe2-4f06-859d-a8d8fd52eb11", "xApple number2", 10, 1, null);
			Prod("27eea6ff-c4aa-44a0-84fe-32b0e13beec4", "xOrange number2", 20, 2, null);
			Prod("dcdc6836-3b44-41c5-8ade-792e39ce2112", "xPeanut butter number2", 25, 2, null);
			Prod("fdc3139d-8585-4b34-b815-13598bf55791", "xApple number2", 10, 1, null);
			Prod("46b058b2-dd55-4741-9ee0-99c63c4bd94a", "xOrange number2", 20, 2, null);
			Prod("11c0e3e5-c5d7-4ff5-ba6c-a54b77f91d1b", "xPeanut butter number2", 25, 2, null);
			Prod("796b6b1d-2d6d-413c-bcab-2659c0fdf02f", "Apple number3", 10, 1, null);
			Prod("5f928f2f-bc1b-4e73-a0d8-20c36f817342", "Orange number3", 20, 2, null);
			Prod("f4fa623d-f412-4b68-8a26-17cfa16cb0a0", "Peanut butter number3", 25, 2, null);
			Prod("8f81d6ab-4c8f-4800-aea0-3cb1ca116fc6", "Apple number4", 10, 1, null);
			Prod("f2b8032f-c8ac-406f-8038-939201771901", "Orange number4", 20, 2, null);
			Prod("dc872dbf-956c-43aa-aba4-e75395af7d83", "Peanut butter number4", 25, 2, null);
			Prod("4ca16ce0-1466-4ba6-a69b-5465ed0a4b1d", "Apple number5", 10, 1, null);
			Prod("66cc487b-2eb1-470b-98c8-03ee913d860e", "Orange number5", 20, 2, null);
			Prod("e77e6589-1dba-4c8b-97b1-1564056ddf10", "Peanut butter number5", 25, 2, null);
			Prod("183946f3-41a9-4140-a1cb-b328e3a778d1", "Apple number6", 10, 1, null);
			Prod("2995edec-ec1b-42ed-aa97-1e68a9ef765c", "Orange number6", 20, 2, null);
			Prod("a78204ae-169a-4d30-936b-034cda5abd0f", "Peanut butter number6", 25, 2, null);
			Context.SaveChanges();
			Order("31d57282-4824-4441-9bd0-49588b952728", "First order", 1);
			Order("fc41cc01-6dc3-4ca5-88a4-c4a463cd6316", "Second order", 1);
			Order("f1379562-c779-43bc-a507-71a8506ca8c9", "Third order", 1);
			Context.SaveChanges();
			await EnsureUsersAsync(true);
			await roleManager.CreateAsync(new IdentityRole("Admin"));
			await roleManager.CreateAsync(new IdentityRole("User"));
			await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("1"), "Admin");
			await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("1"), "User");
			await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync("2"), "User");
		}

		private async Task EnsureUsersAsync(bool setFavouriteProduct)
		{
			await EnsureUserAsync("1", "testy@example.com", "testy@example.com", 7, setFavouriteProduct);
			await EnsureUserAsync("2", "testy@testy.com", "testy@testy.com", 7, setFavouriteProduct);
			await EnsureUserAsync("3", "money@boy.com", "money@boy.com", 7, setFavouriteProduct);
		}

		private async Task EnsureUserAsync(string id, string userName, string email, int? favouriteProductId, bool setFavouriteProdut)
		{
			var user = await _userManager.FindByIdAsync(id);
			Action<ApplicationUser> setFields = user_ =>
			{
				user_.UserName = userName;
				user_.Email = email;
				if (setFavouriteProdut)
				{
					user_.FavouriteProductId = favouriteProductId;
				}
			};
			if (user == null)
			{
				user = new ApplicationUser
				{
					Id = id,
				};
				setFields(user);
				await _userManager.CreateAsync(user);
			}
			else
			{
				setFields(user);
				await _userManager.UpdateAsync(user);
			}
		}

		private int Prod(string guid, string name, double price, int? customerId,
			DateTime? dateCreated, string cratedByUserId = null)
		{
			var entity = _productsCrud.EnsureEntity(
				guid, product =>
				{
					product.Name = name;
					product.Price = price;
					product.CustomerId = customerId;
					product.DateCreated = dateCreated ?? DateTime.UtcNow.AddDays(-ToInt(guid) % 365);
					product.CreatedByUserId = cratedByUserId;
					product.OwnerEmailAddress = "empty@empty.com";
				});
			Context.SaveChanges();
			return entity.ProductId;
		}

		private int ToInt(string guid)
		{
			return guid.ToCharArray().Sum(c => c);
		}

		private void Order(string guid, string title, int customerId)
		{
			_ordersCrud.EnsureEntity(
				guid, entity =>
				{
					entity.Title = title;
					entity.CustomerId = customerId;
				});
		}

		public static async Task MigrateDatabaseAsync(IServiceProvider serviceProvider)
		{
			var dbCreated = EnsureDatabaseExists(false);
			var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
			if (context != null)
			{
				try
				{
					context.Database.Migrate();
				}
				catch (SqlException e)
				{
					if (e.Message == "There is already an object named 'AspNetRoles' in the database.")
					{
						EnsureDatabaseExists(true);
						context.Database.Migrate();
					}
				}
				if (dbCreated)
				{
					await SeedAllAsync(serviceProvider);
				}
			}
			else
			{
				throw new Exception("Unable to resolve database context");
			}
		}

		private static bool EnsureDatabaseExists(bool force = false)
		{
			using (var sqlConnection = new SqlConnection(
				"Server=.;Integrated Security=true;"))
			{
				sqlConnection.Open();
				if (!force)
				{
					var dbExists = new SqlCommand("SELECT db_id('" + DbScript.DbName + "')", sqlConnection).ExecuteScalar() == DBNull.Value;
					force = dbExists;
				}
				if (force)
				{
					foreach (var sql in DbScript.NewDb.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
					{
						new SqlCommand(sql, sqlConnection).ExecuteNonQuery();
					}
				}
				return force;
			}
		}
	}
}