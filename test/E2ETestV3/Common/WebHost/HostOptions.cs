using System;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Host options which controls hosting process which includes:
    /// start, stop and dispose server
    /// </summary>
    public abstract class HostOptions : IDisposable
    {
        public DeploymentOptions DeploymentOptions { get; set; }
        public bool RemoveSiteWhenStop { get; set; }

        public abstract string Start();
        public abstract void Stop();

        public virtual void Dispose()
        {
        }
    }
}
