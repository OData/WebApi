using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				 .AddCommandLine(args)
				 .AddEnvironmentVariables(prefix: "ASPNETCORE_")
				 .Build();

			StartupBase.Init<Startup>(s => s
				.UseConfiguration(config)
				.UseEnvironment("Development")
				.UseKestrel()
				.UseIISIntegration());
		}

		private static void MicroApp(params string[] args)
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				//.UseDefaultHostingConfiguration(args)
				.UseEnvironment("Development")
				//.UseIISPlatformHandlerUrl()
				.UseIISIntegration()
				.Configure(app =>
				{
					app.Use((context, next) =>
					{
						context.Response.Clear();
						context.Response.WriteAsync("Hey");
						return next();
					});
				})
				.Build();
			host.Run();
		}
	}
}