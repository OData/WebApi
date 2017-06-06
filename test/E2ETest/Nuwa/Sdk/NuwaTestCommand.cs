using System;
using System.Xml;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    /// <summary>
    /// the test command adopt specific host strategy
    /// </summary>
    public class NuwaTestCommand : ITestCommand
    {
        private ITestCommand _proxy;

        public NuwaTestCommand(ITestCommand innerCommand)
        {
            if (innerCommand == null)
            {
                throw new ArgumentNullException("innerCommand");
            }

            _proxy = innerCommand;
        }

        public RunFrame Frame { get; set; }

        public IMethodInfo TestMethod { get; set; }

        /// <inheritdoc/>
        public bool ShouldCreateInstance
        {
            get { return _proxy.ShouldCreateInstance; }
        }

        /// <inheritdoc/>
        public int Timeout
        {
            get { return _proxy.Timeout; }
        }

        /// <inheritdoc/>
        public string DisplayName
        {
            get
            {
                return string.Format("{0} [{1}]", _proxy.DisplayName, Frame != null ? Frame.Name : @"N/A");
            }
        }

        /// <inheritdoc/>
        public XmlNode ToStartXml()
        {
            return _proxy.ToStartXml();
        }

        /// <inheritdoc/>
        public MethodResult Execute(object testClass)
        {
            Frame.Initialize(testClass, this);

            // execute delegated test command
            return _proxy.Execute(testClass);
        }
    }
}