using System;
using System.Diagnostics;
using WebStack.QA.Js.Utils;

namespace WebStack.QA.Js.Client
{
    public class PhantomJsRunner
    {
        private const string PhantomjsExeName = "WebStack.QA.Js.phantomjs.exe";

        public PhantomJsRunner(ResourceLoader loader)
        {
            Loader = loader;
            Timeout = 6000;
        }

        public string Output { get; set; }
        public int Timeout { get; set; }
        public ResourceLoader Loader { get; set; }

        public bool Run(string runnerFileName, string testUrl, params string[] args)
        {
            Console.WriteLine("Running tests on " + testUrl);
            var phantomJsExePath = Loader.SaveAsFile(PhantomjsExeName);
            var runnerFilePath = Loader.SaveAsFile(runnerFileName);
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = phantomJsExePath;
            process.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\"", runnerFilePath, testUrl);
            process.Start();
            process.WaitForExit(Timeout);
            Output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(Output);
            return process.ExitCode == 0;
        }
    }
}
