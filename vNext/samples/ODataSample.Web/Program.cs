using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				 .AddEnvironmentVariables(prefix: "ASPNETCORE_")
				 .Build();
			StartupBase.Init<Startup>(host => host
				.UseConfiguration(config)
				//.UseDefaultHostingConfiguration(args)
				.UseIISPlatformHandlerUrl()
				.UseKestrel());
		}
	}
}
