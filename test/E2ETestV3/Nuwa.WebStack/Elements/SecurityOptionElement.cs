using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Nuwa.Sdk.Elements
{
    /// <summary>
    /// Represent a security strategy, which is adopted by a host strategy.
    /// </summary>
    /// 
    /// Note to myself (trdai): At current point this class is just a simple
    /// wrapper around the certificate attribute. There are twp points to do
    /// so: 
    /// 
    /// 1. I don't like the idea of pass attribute around or directly expose
    /// an attribute as a date to be consumed by later procedure. I regard
    /// attribute as syntax element, which simply accept user compile time
    /// input. 
    /// 2. The additional layer of abstract may help in the future when more
    /// security schemas are introduced. The certificate attribute is about 
    /// certificate rather than schema
    internal class SecurityOptionElement : AbstractRunElement
    {
        public SecurityOptionElement(X509Certificate2 cert)
        {
            // TODO: Complete member initialization
            this.Certificate = cert;
            this.Name = "SSL Setup";
        }

        public X509Certificate2 Certificate
        {
            get;
            private set;
        }
    }
}
