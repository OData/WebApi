//-----------------------------------------------------------------------------
// <copyright file="TestServiceFixture.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WebApiPerformance.Test
{
    public class TestServiceFixture: IDisposable
    {
        private const int ServicePort = 9060;
        private const string IISExpressProcessName = "iisexpress";
        private static readonly string IISExpressPath =
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\IIS Express\iisexpress.exe");

        public Uri ServiceBaseUri;

        public TestServiceFixture()
        {
            KillServices();
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = IISExpressPath,
                Arguments = string.Format("/path:{0} /port:{1}", GetServicePath(), ServicePort)
            };

            ServiceBaseUri = new Uri("http://localhost" + ":" + ServicePort + "/");
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start service:" + ServiceBaseUri);
            }

            System.Threading.Thread.Sleep(3000);
        }

        public void Dispose()
        {
            KillServices();
        }

        private void KillServices()
        {
            var processes = Process.GetProcessesByName(IISExpressProcessName);
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        private string GetServicePath()
        {
            var dllPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            return Directory.GetParent(dllPath).FullName;
        }
    }
}
