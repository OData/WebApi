using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Nuwa.DI;
using Nuwa.Sdk;
using Xunit.Sdk;

namespace Nuwa.Control
{
    /// <summary>
    /// Define the class level execution of Nuwa
    /// </summary>
    public class NuwaTestClassCommand : DelegatingTestClassCommand
    {
        private Collection<RunFrame> _frames;
        private IRunFrameBuilder _frmBuilder;

        public NuwaTestClassCommand()
            : base(new TestClassCommand())
        {
            var resolver = DependencyResolver.Instance;

            // autowiring
            _frmBuilder = resolver.Container.Resolve(
                typeof(IRunFrameBuilder),
                new NamedParameter("testClass", this))
                as IRunFrameBuilder;

            _frames = new Collection<RunFrame>();
        }

        public static NuwaFrameworkAttribute GetNuwaFrameworkAttr(ITestClassCommand cmd)
        {
            return cmd.TypeUnderTest.GetFirstCustomAttribute<NuwaFrameworkAttribute>();
        }

        /// <summary>
        /// Act before any test method is executed. All host strategies requested are set up in this method.
        /// </summary>
        /// <returns>Returns exception thrown during execution; null, otherwise.</returns>
        public override Exception ClassStart()
        {
            Exception exception = null;

            try
            {
                ValidateTypeUnderTest();

                // execute the default class start, should any exception returned terminate the execution.
                exception = Proxy.ClassStart();
                if (exception != null)
                {
                    // expected to be catched at upper level try clause
                    throw new InvalidOperationException("Base class ClassStart failed", exception);
                }

                // create run frames
                _frames = _frmBuilder.CreateFrames();
            }
            catch (Exception e)
            {
                exception = e;
            }

            return exception;
        }

        /// <summary>
        /// Act after all test methods are executed. All host strategies are released in this method. 
        /// </summary>
        /// <returns>Returns aggregated exception thrown during execution; null, otherwise.</returns>
        public override Exception ClassFinish()
        {
            Exception retException = null;

            try
            {
                var exceptions = new List<Exception>();

                // dispose all run frame
                foreach (var rf in _frames)
                {
                    try
                    {
                        rf.Cleanup();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                // first release all the hosts
                Exception baseException = Proxy.ClassFinish();
                if (baseException != null)
                {
                    exceptions.Add(baseException);
                }

                if (exceptions.Count != 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
            catch (Exception e)
            {
                retException = e;
            }

            return retException;
        }

        public override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            /// TODO - Advanced feature:
            /// 1. Frame filter, some cases can be filtered under some frame
            var combinations = from test in Proxy.EnumerateTestCommands(testMethod)
                               from frame in _frames
                               select new { TestCommand = test, RunFrame = frame };

            foreach (var each in combinations)
            {
                var isSkipped =
                    (each.TestCommand is DelegatingTestCommand) ?
                    (each.TestCommand as DelegatingTestCommand).InnerCommand is SkipCommand :
                    (each.TestCommand is SkipCommand);

                if (isSkipped)
                {
                    yield return each.TestCommand;
                }
                else
                {
                    var testCommand = new NuwaTestCommand(each.TestCommand)
                    {
                        Frame = each.RunFrame,
                        TestMethod = testMethod
                    };

                    yield return testCommand;
                }
            }
        }

        /// <summary>
        /// Validate the type under test before any actual operation is done. 
        /// 
        /// Exception will be thrown if the validation failed. The thrown exception
        /// is expected be caught in external frame.
        /// </summary>
        private void ValidateTypeUnderTest()
        {
            // check framework attribute
            if (NuwaTestClassCommand.GetNuwaFrameworkAttr(this) == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The test class must be marked by {0}.",
                        typeof(NuwaFrameworkAttribute).Name));
            }

            // check configuration method attribute
            var configMethodAttr = TypeUnderTest.GetCustomAttributes<NuwaConfigurationAttribute>();
            if (configMethodAttr.Length > 1)
            {
                throw new InvalidOperationException(
                    string.Format("More than two methods are marked by {0}.", typeof(NuwaConfigurationAttribute).Name));
            }
        }
    }
}