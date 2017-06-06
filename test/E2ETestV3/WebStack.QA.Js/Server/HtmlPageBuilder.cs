using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Js.Server
{
    public class HtmlPageBuilder
    {
        public HtmlPageBuilder()
        {
            ScriptReferences = new List<string>();
            ScriptCode = new List<string>();
            CssReferences = new List<string>();
            CssCode = new List<string>();
        }

        public HtmlPageBuilder(HtmlPageBuilder builder) : this()
        {
            ScriptReferences.AddRange(builder.ScriptReferences);
            ScriptCode.AddRange(builder.ScriptCode);
            CssReferences.AddRange(builder.CssReferences);
            CssCode.AddRange(builder.CssCode);
            Title = builder.Title;
            BodyContent = builder.BodyContent;
            HeaderContent = builder.HeaderContent;
        }

        public string Title { get; set; }
        public List<string> CssReferences { get; set; }
        public List<string> CssCode { get; set; }
        public List<string> ScriptReferences { get; set; }
        public List<string> ScriptCode { get; set; }
        public string BodyContent { get; set; }
        public string HeaderContent { get; set; }

        public string Build(string root)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">");
            sb.AppendLine(@"<html>");
            sb.AppendLine(@"<head>");
            sb.AppendLine(HeaderContent);
            BuildCssReferences(sb, CssReferences, root);
            BuildCssCode(sb, CssCode);
            BuildScriptReferences(sb, ScriptReferences, root);
            BuildScriptCode(sb, ScriptCode);
            sb.AppendLine(@"</head>");
            sb.AppendLine(@"<body>");
            sb.AppendLine(BodyContent);
            sb.AppendLine(@"</body>");
            sb.AppendLine(@"</html>");

            return sb.ToString();
        }

        private static void BuildScriptCode(StringBuilder sb, List<string> scriptCode)
        {
            foreach (var script in scriptCode)
            {
                sb.AppendLine(@"<script type=""text/javascript"">");
                sb.AppendLine(script);
                sb.AppendLine(@"</script>");  
            }
        }

        private static void BuildScriptReferences(StringBuilder sb, List<string> scriptReferences, string root)
        {
            foreach (var scriptReference in scriptReferences)
            {
                if (scriptReference.StartsWith("http://"))
                {
                    sb.AppendFormat(@"<script type=""text/javascript"" src=""{0}""></script>", scriptReference);
                }
                else
                {
                    sb.AppendFormat(@"<script type=""text/javascript"" src=""{0}/Script?resourceName={1}""></script>", root, scriptReference);
                }
                sb.AppendLine();
            }
        }

        private static void BuildCssCode(StringBuilder sb, List<string> cssCode)
        {
            foreach (var css in cssCode)
            {
                sb.AppendLine(@"<style type=""text/css"">");
                sb.AppendLine(css);
                sb.AppendLine(@"</style>");                
            }
        }

        private static void BuildCssReferences(StringBuilder sb, List<string> cssReferences, string root)
        {
            foreach (var cssReference in cssReferences)
            {
                if (cssReference.StartsWith("http://"))
                {
                    sb.AppendFormat(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}"">", cssReference);
                }
                else
                {
                    sb.AppendFormat(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}/Css?resourceName={1}"">", root, cssReference);
                }
                sb.AppendLine();
            }
        }
    }
}
