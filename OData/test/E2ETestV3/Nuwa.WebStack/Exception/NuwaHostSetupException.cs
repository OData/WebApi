using System;

namespace Nuwa
{
    [Serializable]
    public class NuwaHostSetupException : Exception
    {
        public NuwaHostSetupException()
            : base("Unknown")
        {
        }

        public NuwaHostSetupException(string message)
            : base(message)
        {
        }

        public NuwaHostSetupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}