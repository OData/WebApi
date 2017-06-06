using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Web.Administration;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Helper of operating IIS
    /// </summary>
    public static class IISHelper
    {
        private static readonly int DefaultIISDelay = 999;

        static IISHelper()
        {
            // Read delay from app.config
            int intDelay;
            string strDelay = System.Configuration.ConfigurationManager.AppSettings["iis_delay"];
            if (strDelay == null || Int32.TryParse(strDelay, out intDelay))
            {
                intDelay = DefaultIISDelay;
            }

            SleepDelay = intDelay;
            BaseUrl = "http://localhost/";
            DefaultWebSiteFolder = @"c:\inetpub\wwwroot";
            DefaultWebSiteName = "Default Web Site";
        }

        public static string BaseUrl { get; set; }
        public static int SleepDelay { get; set; }
        public static string DefaultWebSiteFolder { get; set; }
        public static string DefaultWebSiteName { get; set; }

        /// <summary>
        /// Clean up the iis
        /// </summary>
        /// <param name="options">
        /// </param>
        public static void CleanupIIS(WebAppSetupOptions options)
        {
            using (ServerManager manager = new ServerManager())
            {
                Site site = manager.Sites[DefaultWebSiteName];

                Application app = site.Applications.Where((x, i) => x.Path == "/" + options.VirtualDirectory).FirstOrDefault();

                if (app != null)
                {
                    site.Applications.Remove(app);
                    manager.CommitChanges();
                }

                // TODO: Currently disabled for debugging purposes
                // RemoveDirectory(Path.Combine(options.IISRoot, options.vdirName));
            }
        }

        /// <summary>
        /// Set up a web site in IIS according to the given setup option
        /// </summary>
        /// <param name="options">
        /// the options
        /// </param>
        /// <returns>
        /// an url pointing to the new created site
        /// </returns>
        public static string SetupIIS(WebAppSetupOptions options)
        {
            if (options.NeedsGlobalAsax())
            {
                options.GenerateGlobalAsaxForCS();
            }

            using (ServerManager manager = new ServerManager())
            {
                Site site = manager.Sites[DefaultWebSiteName];

                // create site directory as well as bin directory
                string path = Path.Combine(DefaultWebSiteFolder, options.VirtualDirectory);
                EnsureDirectory(path);
                string binPath = Path.Combine(path, "Bin");
                EnsureDirectory(binPath);

                // list the required assemblies and copy
                string[] assembliesToAlwaysCopy = new string[]
                    {
                        Assembly.GetExecutingAssembly().Location, // this assembly
                    };

                foreach (var assembly in assembliesToAlwaysCopy)
                {
                    File.Copy(assembly, Path.Combine(binPath, Path.GetFileName(assembly)), true);
                }

                // generated all text based files. they are mostly configuration files.                
                if (options.TextFiles != null)
                {
                    foreach (var textFileName in options.TextFiles.Keys)
                    {
                        var file = new FileInfo(Path.Combine(path, textFileName));
                        file.Directory.Create();
                        File.WriteAllText(file.FullName, options.TextFiles[textFileName]);
                    }
                }

                // copy all required binary files
                if (options.BinaryFiles != null)
                {
                    foreach (var file in options.BinaryFiles)
                    {
                        File.Copy(file, Path.Combine(binPath, Path.GetFileName(file)), true);
                    }
                }

                // add application to web site
                site.Applications.Add("/" + options.VirtualDirectory, path);
                manager.CommitChanges();

                Thread.Sleep(SleepDelay);

                return new Uri(new Uri(BaseUrl), options.VirtualDirectory).ToString();
            }
        }

        public static void EnableFormsAuthentication()
        {
            SetAuthenticationSection("mode", "Forms");
        }

        public static void ResetAuthentication()
        {
            SetAuthenticationSection("mode", "Windows");
        }

        private static void SetAuthenticationSection(string attributeName, string value)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Site site = serverManager.Sites.Where(s => s.Name == IISHelper.DefaultWebSiteName).Single();
                Configuration config = serverManager.GetWebConfiguration(site.Name);
                ConfigurationSection authenticationSection =
                             config.GetSection("system.web/authentication");

                authenticationSection.SetAttributeValue(attributeName, value);
                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Create a directory based on the given path so as to ensure the directory always exists.
        /// </summary>
        /// <param name="path">
        /// path to the directory
        /// </param>
        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Remove a directory including all files under it.
        /// </summary>
        /// <param name="path">
        /// path to the directory to be removed
        /// </param>
        private static void RemoveDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                Directory.Delete(path, recursive: true);
            }
        }
    }
}