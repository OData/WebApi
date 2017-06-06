using System;

namespace Nuwa
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaHttpClientConfigurationAttribute : Attribute
    {
        public NuwaHttpClientConfigurationAttribute()
        {
            MessageLog = false;
            UseProxy = false;
        }

        /// <summary>
        /// Setting log request and response message during runtime. It is off by default.
        /// 
        /// If this property is set to true, client will print the request and 
        /// response to Console.
        /// </summary>
        public bool MessageLog { get; set; }

        /// <summary>
        /// Enable proxy when sending request. Off by default
        /// 
        /// If this property is true, every request send by the HttpClient will be redirected to
        /// a proxy server to simulate a cross machine request.
        /// </summary>
        public bool UseProxy { get; set; }
    }
}