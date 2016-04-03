using Microsoft.AspNetCore.Hosting;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()
				.UseDefaultHostingConfiguration(args)
				.UseIISPlatformHandlerUrl()
				.UseKestrel()
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}
	}
}
