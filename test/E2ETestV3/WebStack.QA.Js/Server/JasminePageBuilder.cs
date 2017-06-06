using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Js.Server
{
    public class JasminePageBuilder : HtmlPageBuilder
    {
        public JasminePageBuilder() : base()
        {
            Title = "Jasmine Spec Runner";
            CssReferences.Add("http://pivotal.github.com/jasmine/lib/jasmine.css");
            ScriptReferences.Add("http://pivotal.github.com/jasmine/lib/jasmine.js");
            ScriptReferences.Add("http://pivotal.github.com/jasmine/lib/jasmine-html.js");
            ScriptCode.Add(
@"
(function() {
      var jasmineEnv = jasmine.getEnv();
      jasmineEnv.updateInterval = 1000;

      var htmlReporter = new jasmine.HtmlReporter();

      jasmineEnv.addReporter(htmlReporter);

      jasmineEnv.specFilter = function(spec) {
        return htmlReporter.specFilter(spec);
      };

      var currentWindowOnload = window.onload;

      window.onload = function() {
        if (currentWindowOnload) {
          currentWindowOnload();
        }
        execJasmine();
      };

      function execJasmine() {
        jasmineEnv.execute();
      }

    })();");
        }
    }
}
