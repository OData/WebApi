using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Web.Mvc
{
    [Serializable]
    [TypeForwardedFrom("System.Web.Mvc, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class HttpAntiForgeryException : HttpException
    {
        public HttpAntiForgeryException()
        {
        }

        private HttpAntiForgeryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public HttpAntiForgeryException(string message)
            : base(message)
        {
        }

        public HttpAntiForgeryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
