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
        private static bool? _enabled;

        private static bool IsEnabled()
        {
            if (_enabled == null)
            {
                bool enabled;
                if (Boolean.TryParse(Environment.GetEnvironmentVariable("RAZOR_EDITOR_TRACE"), out enabled))
                {
                    Trace.WriteLine(String.Format(
                        "[RzEd] Editor Tracing {0}",
                        enabled ? "Enabled" : "Disabled"));
                    _enabled = enabled;
                }
                else
                {
                    _enabled = false;
                }
            }
            return _enabled.Value;
        }

        [Conditional("EDITOR_TRACING")]
        public static void TraceLine(string format, params object[] args)
        {
            if (IsEnabled())
            {
                Trace.WriteLine(String.Format(
                    "[RzEd] {0}",
                    String.Format(format, args)));
            }
        }

        private static string FormatList(IEnumerable<TextChange> changes)
        {
            return String.Join(",", changes.Select(c => c.ToString()));
        }
    }
}
