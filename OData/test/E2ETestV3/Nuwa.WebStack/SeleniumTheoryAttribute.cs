using Nuwa.WebStack.Browser;
using System.Collections.Generic;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Nuwa
{
    public class NuwaTheoryAttribute : TheoryAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return BrowserTestCommandHelper.EnumerateTestCommands(method, base.EnumerateTestCommands(method));
        }
    }
}
