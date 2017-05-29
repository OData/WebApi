using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Js.Server
{
    public class QUnitPageBuilder : HtmlPageBuilder
    {
        public QUnitPageBuilder()
            : base()
        {
            Title = "Qunit Runner";
            CssReferences.Add("http://code.jquery.com/qunit/qunit-1.11.0.css");
            ScriptReferences.Add("http://code.jquery.com/qunit/qunit-1.11.0.js");
            BodyContent =
@"
<div id=""qunit""></div>
<div id=""qunit-fixture""></div>
";
        }
    }
}
