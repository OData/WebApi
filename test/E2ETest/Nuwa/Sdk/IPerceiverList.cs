using System.Collections.Generic;

namespace Nuwa.Sdk
{
    /// <summary>
    /// Returns the list of perceivers
    /// </summary>
    public interface IPerceiverList
    {
        IList<IRunElementPerceiver> Perceivers
        {
            get;
        }
    }
}