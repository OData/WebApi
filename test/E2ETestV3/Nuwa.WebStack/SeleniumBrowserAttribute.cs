using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Nuwa.WebStack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;

namespace Nuwa
{
    /// <summary>
    /// NuwaBrowserAttribute is used to mark a property in the test class. To the
    /// property, test framework will assign IWebDriver instance. The framework created
    /// browser instance based on the app settings or code configuration
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SeleniumBrowserAttribute : Attribute
    {
        public SeleniumBrowserAttribute()
        {
        }

        public SeleniumBrowserAttribute(BrowserTypes browsers)
        {
            Browsers = browsers;
        }

        public BrowserTypes? Browsers { get; set; }

        public IEnumerable<Func<IWebDriver>> GetBrowserCreators()
        {
            BrowserTypes browserTypes;
            if (!Browsers.HasValue)
            {
                if (string.IsNullOrEmpty(NuwaGlobalConfiguration.Browsers))
                {
                    throw new ConfigurationErrorsException(string.Format("{0} can't be null", NuwaGlobalConfiguration.BrowsersKey));
                }

                if (!Enum.TryParse<BrowserTypes>(NuwaGlobalConfiguration.Browsers, out browserTypes))
                {
                    throw new ConfigurationErrorsException(string.Format(
                        "{0} can't be parsed. Please correct it's syntax. For example: Firefox, IE, Chrome, Safari",
                        NuwaGlobalConfiguration.BrowsersKey));
                }
            }
            else
            {
                browserTypes = Browsers.Value;
            }

            if (browserTypes.HasFlag(BrowserTypes.Firefox))
            {
                yield return CreateFireFoxDriver;
            }

            if (browserTypes.HasFlag(BrowserTypes.IE))
            {
                yield return CreateInternetExplorerDriver;
            }

            if (browserTypes.HasFlag(BrowserTypes.Chrome))
            {
                yield return CreateChromeDriver;
            }

            if (browserTypes.HasFlag(BrowserTypes.Safari))
            {
                yield return CreateSafariDriver;
            }
        }

        /// <summary>
        /// Returns the property marked with SeleniumBrowserAttribute.
        /// 
        /// There must be either none or only one property qualified.
        /// </summary>
        /// <param name="type">The type on which to search for the property</param>
        /// <returns>The property info</returns>
        public static PropertyInfo GetBrowserProperty(Type type)
        {
            var browserProps = type.GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(SeleniumBrowserAttribute), false).Length == 1);

            if (!browserProps.Any())
            {
                return null;
            }

            if (browserProps.Count() > 1)
            {
                throw new InvalidOperationException("You can't define multiple NuwaBrowserAttribute properties in one test class.");
            }

            return browserProps.Single();
        }

        protected virtual FirefoxDriver CreateFireFoxDriver()
        {
            return new FirefoxDriver();
        }

        protected virtual InternetExplorerDriver CreateInternetExplorerDriver()
        {
            return new InternetExplorerDriver();
        }

        protected virtual ChromeDriver CreateChromeDriver()
        {
            return new ChromeDriver();
        }

        protected virtual SafariDriver CreateSafariDriver()
        {
            return new SafariDriver();
        }
    }
}
