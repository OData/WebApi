using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Web.Administration;

namespace WebStack.QA.Common.WebHost
{
    public static class SecurityHelper
    {
        public static void CleanupEnvironment(string certificateFilePath, string port, string addressSuffix)
        {
            // CLEANUP
            Run(string.Format(@"netsh http delete urlacl url=https://+:{0}/{1}", port, addressSuffix));
            Run(string.Format(@"netsh http delete sslcert ipport=0.0.0.0:{0}", port));

            // Remove the certificate from store
            RemoveCertificate(certificateFilePath);
        }

        public static X509Certificate2 PrepareEnvironment(string certificateFilePath, string port, string username, string addressSuffix)
        {
            // STEP 1: install the certificate to Local Machine store
            var certificate = RegisterCertificate(certificateFilePath);

            // STEP 2: Map the SSL cert to the WCF listening port
            var strb = new StringBuilder();
            certificate.GetCertHash().ToList().ForEach(bite => strb.AppendFormat("{0:X2}", bite));

            Run(string.Format("netsh http add sslcert ipport=0.0.0.0:{0} certhash={1} appid={{{2}}}",
                port, strb.ToString(), "7cb8543c-30f6-4d38-a704-a8234df1a837"));

            // STEP 3:  Set an URL ACL for the listening URI
            // netsh http add urlacl url=https://+:8843/SelfHostSecuritySample user=domain\username
            Run(string.Format(@"netsh http add urlacl url=https://+:{0}/{1} user={2}",
                port, addressSuffix, username));

            return certificate;
        }

        /// <summary>
        /// Install a certificate to local machine store
        /// </summary>
        /// <param name="certificateFilePath"></param>
        /// <returns>Certificate added to the store.</returns>
        public static X509Certificate2 RegisterCertificate(string certificateFilePath)
        {
            // STEP 1: install the certificate to Local Machine store
            if (!File.Exists(certificateFilePath))
            {
                throw new ArgumentException("Certificate file doesn't exist.");
            }

            var certificate = new X509Certificate2(
                certificateFilePath, "1234", X509KeyStorageFlags.MachineKeySet);

            var store = new X509Store(StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadWrite);
                var results = store.Certificates.Find(X509FindType.FindBySerialNumber, certificate.SerialNumber, true);
                if (results.Count == 0)
                {
                    store.Add(certificate);
                }
            }
            finally
            {
                store.Close();
            }

            return certificate;
        }

        public static void RemoveCertificate(string certificateFilePath)
        {
            var certificate = new X509Certificate2(
                certificateFilePath, "1234", X509KeyStorageFlags.MachineKeySet);

            var store = new X509Store(StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadWrite);
                var results = store.Certificates.Find(X509FindType.FindBySerialNumber, certificate.SerialNumber, true);
                if (results.Count != 0)
                {
                    foreach (var each in results)
                    {
                        store.Remove(each);
                    }
                }
            }
            finally
            {
                store.Close();
            }
        }

        public static void AddIpListen()
        {
            Run(string.Format(@"netsh http add iplisten ipaddress=::"));
        }

        public static bool AddHttpsBinding(X509Certificate2 cert, string port)
        {
            using (var mgr = new ServerManager())
            {
                var site = mgr.Sites[IISHelper.DefaultWebSiteName];
                foreach (var each in site.Bindings)
                {
                    if (each.BindingInformation == "*:" + port + ":")
                    {
                        return true;
                    }
                }

                site.Bindings.Add("*:" + port + ":", cert.GetCertHash(), "MY");
                mgr.CommitChanges();
            }

            return false;
        }

        public static void RemoveHttpsBinding(string port)
        {
            using (var mgr = new ServerManager())
            {
                var site = mgr.Sites[IISHelper.DefaultWebSiteName];
                Binding toDelete = null;
                foreach (var each in site.Bindings)
                {
                    if (each.BindingInformation == "*:" + port + ":")
                    {
                        toDelete = each;
                    }
                }

                if (toDelete != null)
                {
                    site.Bindings.Remove(toDelete);
                    mgr.CommitChanges();
                }
            }
        }

        private static void Run(string command)
        {
            var process = Process.Start(new ProcessStartInfo("cmd", "/c " + command)
            {
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            Console.WriteLine(command);
            Console.Write(process.StandardOutput.ReadToEnd());
            Console.Write(process.StandardError.ReadToEnd());
        }
    }
}
