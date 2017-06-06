using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuwa.Sdk;
using Nuwa.Sdk.Elements;

namespace Nuwa.WebStack.Factory
{
    public class WebStackRunFrameFactory : AbstractRunFrameFactory
    {
        public override RunFrame CreateFrame(IEnumerable<IRunElement> elements)
        {
            return new RunFrame(elements, GetName(elements));
        }

        private string GetName(IEnumerable<IRunElement> elements)
        {
            List<string> clues = new List<string>();

            var host = elements.FirstOrDefault(e => e is BaseHostElement);
            if (host != null)
            {
                clues.Add(host.Name);
            }

            var trace = elements.FirstOrDefault(e => e is TraceElement);
            if (trace != null)
            {
                clues.Add(trace.Name);
            }

            var name = string.Join(" ", clues);

            if (string.IsNullOrEmpty(name))
            {
                name = "Unknown";
            }

            return name;
        }
    }
}
