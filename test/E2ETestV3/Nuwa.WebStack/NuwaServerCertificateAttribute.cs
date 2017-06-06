using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Nuwa
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaServerCertificateAttribute : Attribute
    {
        private X509Certificate2 _cert;
        private string _certFilepath;
        private string _certPassword;

        public NuwaServerCertificateAttribute(string file, string password = null)
        {
            _certFilepath = file;
            _certPassword = password ?? string.Empty;
        }

        public X509Certificate2 Certificate
        {
            get
            {
                if (_cert == null && File.Exists(_certFilepath))
                {
                    _cert = new X509Certificate2(_certFilepath, _certPassword, X509KeyStorageFlags.MachineKeySet);
                }

                return _cert;
            }
        }
    }
}