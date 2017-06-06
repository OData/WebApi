using System;
using System.Net;

namespace Nuwa
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NuwaNetworkCredentialAttribute : Attribute
    {
        private Lazy<NetworkCredential> _credential;

        /// <summary>
        /// Log client by a NetworkCredential
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        public NuwaNetworkCredentialAttribute(string username, string password)
        {
            _credential = new Lazy<NetworkCredential>(() => new NetworkCredential(username, password), true);
        }

        /// <summary>
        /// Log client by a NetworkCredential
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="password">the password</param>
        /// <param name="domain">the domain</param>
        public NuwaNetworkCredentialAttribute(string username, string password, string domain)
        {
            _credential = new Lazy<NetworkCredential>(() => new NetworkCredential(username, password, domain), true);
        }

        public NetworkCredential Credential
        {
            get { return _credential.Value; }
        }
    }
}