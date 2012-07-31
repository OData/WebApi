using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor.Text;

namespace System.Web.Razor.Editor
{
    internal static class RazorEditorTrace
    {
        private static bool IsEnabled()
        {
            bool enabled;
            return Boolean.TryParse(Environment.GetEnvironmentVariable("RAZOR_EDITOR_TRACE"), out enabled) && enabled;
        }

        private static void TraceLine(string format, params object[] args)
        {
            if (IsEnabled())
            {
                Trace.WriteLine(String.Format(
                    "[RzEd] {0}",
                    String.Format(format, args)));
            }
        }

        [Conditional("DEBUG")]
        public static void TreeStructureHasChanged(bool treeStructureChanged, TextChange[] changes)
        {
            if (treeStructureChanged)
            {
                TraceLine("Tree changed after: {0}", FormatList(changes));
            }
        }

        private static string FormatList(TextChange[] changes)
        {
            return String.Join(",", changes.Select(c => c.ToString()));
        }
    }
}
