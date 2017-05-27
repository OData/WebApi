using System;
using System.IO;
using Brandless.EntityFrameworkCore.Migrations;
using ODataSample.Web.Models;
using System.Linq;

namespace ODataSample.Web.Migrations
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string[] possibleProjectPaths =
			{
				@"D:\Code\Git\Forks\OData-WebApi\vNext\samples\ODataSample",
				@"D:\B\Forks\OData-WebApi\vNext\samples\ODataSample"
			};
			var projectPath = possibleProjectPaths.First(Directory.Exists);
			var mig = new CodeFirstMigrations<ApplicationDbContext>(projectPath);
			Console.WriteLine("Please enter the name of this migration:");
			var name = Console.ReadLine();
			name = string.IsNullOrWhiteSpace(name) ? "" : name;
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("No name provided for the migration");
			}
			mig.Add(name);
		}
	}
}
