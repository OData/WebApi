using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using LTAF.Infrastructure;

namespace WebStack.QA.Common.WebHost
{
    // TODO: LTAF has a bug that it can't start multiple host at same time. So the class has to use different pattern as IIS express.
    // After it's fixed, they can share almost same code.

    /// <summary>
    /// Use LTAF to setup IIS host.
    /// </summary>
    public class IISHostOptions : HostOptions
    {
        private WebServerSettings _settings;

        public IISHostOptions(DirectoryInfo physicalDirectory, string virtualPath)
        {
            if (physicalDirectory == null)
            {
                throw new ArgumentNullException("physicalDirectory");
            }

            if (string.IsNullOrEmpty(virtualPath))
            {
                throw new ArgumentNullException("virtualPath");
            }

            PhysicalDirectory = physicalDirectory;
            VirtualPath = virtualPath;
            Users = new List<string>();
            IISDelay = 999;
        }

        public DirectoryInfo PhysicalDirectory { get; set; }
        public string VirtualPath { get; set; }
        public List<string> Users { get; set; }
        public int IISDelay { get; set; }

        public override string Start()
        {
            PhysicalDirectory.Refresh();
            if (!PhysicalDirectory.Exists)
            {
                throw new DirectoryNotFoundException(PhysicalDirectory.FullName);
            }

            SetUserPermission(Users, PhysicalDirectory);

            _settings = new WebServerSettings();
            _settings.RootPhysicalPath = PhysicalDirectory.FullName;

            using (var server = new WebServerIIS(_settings))
            {
                server.Start();
                var application = server.DefaultWebSite.Applications.Add("/" + VirtualPath, PhysicalDirectory.FullName);
                server.ServerManager.CommitChanges();

                Thread.Sleep(IISDelay);

                return server.DefaultWebSite.GetHttpVirtualPath() + application.Path;
            }

        }

        private void SetUserPermission(IEnumerable<string> users, DirectoryInfo directory)
        {
            foreach (var user in users)
            {
                AddDirectorySecurity(directory, user, FileSystemRights.Read, AccessControlType.Allow);
            }
        }

        public override void Stop()
        {
            using (var server = new WebServerIIS(_settings))
            {
                var app = server.DefaultWebSite.Applications.FirstOrDefault(a => a.Path == "/" + VirtualPath);
                if (app != null)
                {
                    server.DefaultWebSite.Applications.Remove(app);
                    server.ServerManager.CommitChanges();
                }
                server.Stop();
            }

            if (RemoveSiteWhenStop)
            {
                PhysicalDirectory.Delete(true);
            }
        }

        // Adds an ACL entry on the specified directory for the specified account. 
        private static void AddDirectorySecurity(DirectoryInfo dInfo, string acount, FileSystemRights rights, AccessControlType controlType)
        {
            // Get a DirectorySecurity object that represents the  
            // current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(new FileSystemAccessRule(acount,
                                                            rights,
                                                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                                                            PropagationFlags.None,
                                                            controlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }
    }
}
