using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Js.Client
{
    public class JasmineTestRunner : JsRunnerBase
    {
        public JasmineTestRunner(string baseUrl)
            : base(baseUrl)
        {
        }

        public override string RunnerFileName
        {
            get 
            {
                return "WebStack.QA.Js.Scripts.run-jasmine.js";
            }
        }
    }
}
