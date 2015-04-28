using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Nuwa.WebStack.Browser
{
    public static class BrowserTestCommandHelper
    {
        public static IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method, IEnumerable<ITestCommand> commands)
        {
            var attr = FindNuwaBrowserAttribute(method.Class.Type);

            if (attr == null)
            {
                return commands;
            }

            var browserCommands = from creator in attr.GetBrowserCreators()
                                  from command in commands
                                  select new BrowserDelegateCommand(creator, command, method);

            return browserCommands;
        }

        private static SeleniumBrowserAttribute FindNuwaBrowserAttribute(Type type)
        {
            var prop = SeleniumBrowserAttribute.GetBrowserProperty(type);
            if (prop == null)
            {
                return null;
            }
            else
            {
                return prop.GetCustomAttributes(typeof(SeleniumBrowserAttribute), false).Single() as SeleniumBrowserAttribute;
            }
        }
    }
}
