using System.Collections.Generic;
using System.Linq;

namespace Nuwa.WebStack.Host
{
    public class PortArranger : IPortArranger
    {
        private Queue<string> _available;
        private HashSet<string> _reserved;

        public PortArranger()
        {
            _available = new Queue<string>(Enumerable.Range(20001, 100).Select(i => i.ToString()));
            _reserved = new HashSet<string>();
        }

        public string Reserve()
        {
            var retval = _available.Dequeue();

            _reserved.Add(retval);

            return retval;
        }

        public void Return(string port)
        {
            if (_reserved.Contains(port))
            {
                _reserved.Remove(port);
            }

            _available.Enqueue(port);
        }
    }
}