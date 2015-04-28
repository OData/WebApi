using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    /// <summary>
    /// Serve as the base class for the those TestClassCommand which delegate to inner class command.
    /// </summary>
    public class DelegatingTestClassCommand : ITestClassCommand
    {
        private ITestClassCommand _proxy;

        /// <summary>
        /// Create a new instance of the <see cref="DelegatingTestClassCommand"/> class.
        /// </summary>
        /// <param name="proxy">The inner command to delegate to.</param>
        public DelegatingTestClassCommand(ITestClassCommand proxy)
        {
            _proxy = proxy;
        }

        protected ITestClassCommand Proxy
        {
            get { return _proxy; }
        }

        /// <inheritdoc/>
        public virtual int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            return Proxy.ChooseNextTest(testsLeftToRun);
        }

        /// <inheritdoc/>
        public virtual Exception ClassFinish()
        {
            return Proxy.ClassFinish();
        }

        /// <inheritdoc/>
        public virtual Exception ClassStart()
        {
            return Proxy.ClassStart();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            return Proxy.EnumerateTestCommands(testMethod);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            return Proxy.EnumerateTestMethods();
        }

        /// <inheritdoc/>
        public virtual bool IsTestMethod(IMethodInfo testMethod)
        {
            return Proxy.IsTestMethod(testMethod);
        }

        /// <inheritdoc/>
        public virtual object ObjectUnderTest
        {
            get { return Proxy.ObjectUnderTest; }
        }

        /// <inheritdoc/>
        public virtual ITypeInfo TypeUnderTest
        {
            get { return Proxy.TypeUnderTest; }
            set { Proxy.TypeUnderTest = value; }
        }
    }
}