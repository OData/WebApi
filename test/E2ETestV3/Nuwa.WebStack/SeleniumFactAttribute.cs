using Nuwa.WebStack.Browser;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Nuwa
{
    public class SeleniumFactAttribute : FactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return BrowserTestCommandHelper.EnumerateTestCommands(method, base.EnumerateTestCommands(method));
        }
    }
}
