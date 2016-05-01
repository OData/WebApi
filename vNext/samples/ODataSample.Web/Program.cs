using Microsoft.AspNetCore.Hosting;

namespace ODataSample.Web
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			StartupBase.Init<Startup>(host => host.UseIISPlatformHandlerUrl());
		}
	}
}
