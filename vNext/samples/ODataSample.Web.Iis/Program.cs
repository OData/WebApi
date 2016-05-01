using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			StartupBase.Init<Startup>(s => s
				.UseEnvironment("Development")
				.UseIISIntegration());
		}

		private static void MicroApp(params string[] args)
		{
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseDefaultHostingConfiguration(args)
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