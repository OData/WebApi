using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Js.Client
{
    public abstract class JsRunnerBase : PhantomJsRunner
    {
        public JsRunnerBase() 
            : base(new Utils.ResourceLoader())
        {
            Loader.LoadFrom.Add(this.GetType().Assembly);
        }

        public JsRunnerBase(string baseUrl)
            : this()
        {
            BaseUrl = baseUrl;
        }

        public string BaseUrl { get; set; }
        public abstract string RunnerFileName { get; }

        public bool Run(string testUrl)
        {
            return base.Run(RunnerFileName, testUrl);
        }

        public bool RunJsFile(string fileName)
        {
            return Run(BaseUrl + "?file=" + fileName);
        }

        public bool RunCode(string code)
        {
            return Run(BaseUrl + "?code=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(code)));
        }
    }
}
