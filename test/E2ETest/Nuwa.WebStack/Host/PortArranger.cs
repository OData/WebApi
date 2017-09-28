using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Web.Management;

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
                _available = new ConcurrentQueue<string>(Enumerable.Range(int.Parse(startPort), 3000).Select(i => i.ToString()));
            }
        }

        /// <summary>
        /// Enum type for active ports
        /// </summary>
        private enum PortTypes
        {
            TCPActiveConnection,
            TCPActiveListener,
            UDPActiveListener
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
            int portOfInt = int.Parse(port);

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // Check the port with active TCP connections.
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            if (connections.Any(c => c.LocalEndPoint.Port == portOfInt || c.RemoteEndPoint.Port == portOfInt))
            {
                LogEvent(portOfInt, PortTypes.TCPActiveConnection, connections.Length);
                return false;
            }

            // Check the port with active TCP listeners.
            IPEndPoint[] tcpListeningEndpoints = properties.GetActiveTcpListeners();
            if (tcpListeningEndpoints.Any(ep => ep.Port == portOfInt))
            {
                LogEvent(portOfInt, PortTypes.TCPActiveListener, tcpListeningEndpoints.Length);
                return false;
            }

            // Check the port with active UDP listeners
            // We shouldn't need to check with UDP ports for current usage, but just in case for general purpose...
            IPEndPoint[] udpListeningEndpoints = properties.GetActiveUdpListeners();
            if (udpListeningEndpoints.Any(ep => ep.Port == portOfInt))
            {
                LogEvent(portOfInt, PortTypes.UDPActiveListener, udpListeningEndpoints.Length);
                return false;
            }

            // pass all checks, port is free to use.
            return true;
        }

        private static void LogEvent(int portNum, PortTypes activePortType, int totalCount)
        {
            EventLog appLog = new EventLog();
            appLog.Source = "Nuwa Katana Self Host Test";
            appLog.WriteEntry(string.Format("Port: [{0}] is already used by: [type: {1}, count: {2}]\n", portNum, activePortType, totalCount),
                EventLogEntryType.Error);
        }
    }
}