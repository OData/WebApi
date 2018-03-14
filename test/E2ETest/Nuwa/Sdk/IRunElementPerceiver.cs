using System.Collections.Generic;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    /// <summary>
    /// A RunElementPerceiver (perceiver) is used to percieve a collection of 
    /// related RunElements (elements) from a given NuwaTestClassCommand (test
    /// class). The group of returned elements mutually exclusive, and representing
    /// an aspects of the setting up environment for test cases.
    /// 
    /// For example, a HostElementsPreceiver will return the group of host strategy
    /// related elements. If the given test class suggests its test cases shall be
    /// run under self-host and web-host, then two elements will be returned..
    /// </summary>
    public interface IRunElementPerceiver
    {
        IEnumerable<IRunElement> Perceive(ITestClassCommand ntcc);
    }

    /// <summary>
    /// Extension helper function to the IRunElement
    /// </summary>
    public static class IRunElementPerceiverExtensions
    {
        public static T[] ToArray<T>(this IRunElementPerceiver self, T one)
        {
            return new T[] { one };
        }

        public static IRunElement[] Empty(this IRunElementPerceiver self)
        {
            return new IRunElement[0];
        }
    }
}