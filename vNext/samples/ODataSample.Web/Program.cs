using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;

namespace ODataSample.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostingConfiguration = WebApplicationConfiguration.GetDefault(args);
 
            var application = new WebApplicationBuilder()
                .UseConfiguration(hostingConfiguration)
                .UseStartup<Startup>()
                .Build();
 
            application.Run();
        }
    }
}
