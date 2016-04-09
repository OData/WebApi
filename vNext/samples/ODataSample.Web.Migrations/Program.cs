using Brandless.EntityFrameworkCore.Migrations;
using ODataSample.Web.Models;

namespace ODataSample.Web.Migrations
{
	public class Program
	{
		public static void Main(string[] args)
		{
			const string projectPath = @"D:\B\Forks\OData-WebApi\vNext\samples\ODataSample.Web";
			var mig = new CodeFirstMigrations<ApplicationDbContext>(projectPath);
			mig.Add("ProductCustomerNullable");
		}
	}
}
