using System;
using Xunit.Extensions;

namespace WebStack.QA.Common.XUnitTest
{
    /// <summary>
    /// Define an InlineData which is skippable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SkippableInlineDataAttribute : InlineDataAttribute, ISkippable
    {
        public SkippableInlineDataAttribute(params object[] dataValues)
            : base(dataValues)
        {
        }

        public string SkipReason { get; set; }
    }
}