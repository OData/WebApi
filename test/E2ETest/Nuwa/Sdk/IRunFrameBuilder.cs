using System.Collections.ObjectModel;

namespace Nuwa.Sdk
{
    /// <summary>
    /// Interface of the component which builds RunFrame
    /// </summary>
    public interface IRunFrameBuilder
    {
        Collection<RunFrame> CreateFrames();
    }
}