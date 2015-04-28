using System;
using System.Web.Http.Tracing;

namespace Nuwa
{
    public class NuwaTraceAttributeException : Exception
    {
        private const string ErrorMessageFormat = @"The type {0} doesn't implement interface {1}";

        public NuwaTraceAttributeException(Type wrongType)
            : base(string.Format(ErrorMessageFormat, wrongType.FullName, typeof(ITraceWriter).FullName))
        {
        }
    }
}