using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Nuwa.WebStack.Host
{
    public class PortArranger : IPortArranger
    {
        private static ConcurrentQueue<string> _available = null;

        static PortArranger()
        {
            string startPort = NuwaGlobalConfiguration.KatanaSelfStartingPort;
            if (_available == null)
            {
                _available = new ConcurrentQueue<string>(Enumerable.Range(int.Parse(startPort), 1000).Select(i => i.ToString()));
            }
        }

        public string Reserve()
        {
            while (true)
            {
                var retval = this.GetPort();

                if (IsFree(retval))
                {
                    return retval;
                }
            }
        }

        public void Return(string port)
        {
            _available.Enqueue(port);
        }

        private string GetPort()
        {
            int repeat = 10;
            int delay = 100;

            for (int i = 0; i < repeat; i++)
            {
                string port;

                if (_available.TryDequeue(out port))
                {
                    return port;
                }

                Thread.Sleep(delay);
            }

            throw new TimeoutException(string.Format("Cannot get an available port in {0}ms.", repeat * delay));
        }

        private static bool IsFree(string port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            int portOfInt = int.Parse(port);
            var isInUse = connections.Any(c =>
                c.LocalEndPoint.Port == portOfInt || c.RemoteEndPoint.Port == portOfInt);
            return !isInUse;
        }
    }
}