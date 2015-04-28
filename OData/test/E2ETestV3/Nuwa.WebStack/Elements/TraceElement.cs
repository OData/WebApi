using System;

namespace Nuwa.Sdk.Elements
{
    internal class TraceElement : AbstractRunElement
    {
        public TraceElement(Type type)
        {
            this.TracerType = type;

            if (TracerType != null)
            {
                this.Name = "Trace:" + TracerType.Name;
            }
            else
            {
                this.Name = "Trace:<NULL>";
            }
        }

        public Type TracerType
        {
            get;
            private set;
        }
    }
}