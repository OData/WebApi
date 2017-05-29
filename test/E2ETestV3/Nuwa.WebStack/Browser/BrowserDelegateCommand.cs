using OpenQA.Selenium;
using System;
using Xunit.Sdk;

namespace Nuwa.WebStack.Browser
{
    public class BrowserDelegateCommand : TestCommand
    {
        private Func<IWebDriver> _creator;
        private ITestCommand _inner;

        public BrowserDelegateCommand(Func<IWebDriver> creator, ITestCommand inner, IMethodInfo method)
            : base(method, inner.DisplayName, inner.Timeout)
        {
            _creator = creator;
            _inner = inner;
        }

        public override MethodResult Execute(object testClass)
        {
            var testType = testClass.GetType();
            var browserProp = SeleniumBrowserAttribute.GetBrowserProperty(testType);
            using (var browser = _creator())
            {
                browserProp.SetValue(testClass, browser);
                return _inner.Execute(testClass);
            }
        }
    }
}
