//-----------------------------------------------------------------------------
// <copyright file="PortArranger.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    public class PortArranger
    {
        private static int nextPort = 11000;

        public static int Reserve()
        {
            int attempts = 0;
            // Scan a generous number of candidate ports before giving up. Under parallel test
            // execution many servers start at once and batch/DoS tests leave numerous sockets in
            // TIME_WAIT, so short runs of consecutive ports can all appear busy; a small retry
            // budget (the original was 10) intermittently threw "Cannot get an available port".
            while (attempts++ < 200)
            {
                int port = Interlocked.Increment(ref nextPort);
                if (port >= 65535)
                {
                    throw new OverflowException("Cannot get an available port, port value overflowed");
                }

                if (IsFree(port))
                {
                    return port;
                }
            }

            throw new TimeoutException(string.Format("Cannot get an available port in {0} attempts.", attempts));
        }

        private static bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            var isInUse = connections.Any(c =>
                c.LocalEndPoint.Port == port || c.RemoteEndPoint.Port == port);
            return !isInUse;
        }
    }
}
