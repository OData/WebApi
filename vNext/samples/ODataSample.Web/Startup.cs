using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace ODataSample.Web
{
	public class Startup : StartupBase
	{
		public Startup(IHostingEnvironment env)
			: base(env, "development")
		{
		}
	}
}