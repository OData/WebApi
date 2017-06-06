using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Extensions;
using Xunit.Sdk;

namespace WebStack.QA.Common.XUnitTest
{
    /// <summary>
    /// Extended theory attribute allow inline data skippable
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExtendedTheoryAttribute : TheoryAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var commands = base.EnumerateTestCommands(method);
            var attrs = method.MethodInfo.GetCustomAttributes(typeof(DataAttribute), false);

            if (commands.Count() != attrs.Count())
            {
                throw new InvalidOperationException("Some data attribute doesn't generate test command");
            }

            var filteredCommands = new List<ITestCommand>();
            int index = 0;

            foreach (var command in commands)
            {
                var theoryCmd = command as TheoryCommand;
                var skippableData = attrs.ElementAt(index++) as ISkippable;
                if (skippableData != null &&
                    !string.IsNullOrEmpty(skippableData.SkipReason))
                {
                    SkipCommand cmd = new SkipCommand(method, theoryCmd.DisplayName, skippableData.SkipReason);
                    filteredCommands.Add(cmd);
                }
                else
                {
                    filteredCommands.Add(command);
                }
            }

            return filteredCommands;
        }
    }
}