using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ODataSample.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var application = new WebHostBuilder()
                .UseDefaultConfiguration(args)
                .UseStartup<Startup>()
                .Build();
 
            application.Run();
        }
    }
}
