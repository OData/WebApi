using System.Collections.Generic;

namespace Nuwa.Sdk
{
    public abstract class AbstractRunFrameFactory
    {
        public abstract RunFrame CreateFrame(IEnumerable<IRunElement> elements);
    }
}
