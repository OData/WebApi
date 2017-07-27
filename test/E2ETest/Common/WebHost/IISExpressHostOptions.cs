using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using LTAF.Infrastructure;

namespace WebStack.QA.Common.WebHost
{

    /// <summary>
    /// Use LTAF to setup IISexpress host.
    /// </summary>
    public class IISExpressHostOptions : HostOptions
    {
        private WebServer _server;

        public IISExpressHostOptions(DirectoryInfo physicalDirectory, string virtualPath)
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

            var settings = new WebServerSettings();
            settings.RootPhysicalPath = PhysicalDirectory.FullName;

            _server = new WebServerIISExpress(settings);
            var application = _server.DefaultWebSite.Applications.Add("/" + VirtualPath, PhysicalDirectory.FullName);
            _server.ServerManager.CommitChanges();

            Thread.Sleep(IISDelay);

            return _server.DefaultWebSite.GetHttpVirtualPath() + application.Path;

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
            _server.Stop();
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

        public override void Dispose()
        {
            _server.Dispose();
        }
    }
}
