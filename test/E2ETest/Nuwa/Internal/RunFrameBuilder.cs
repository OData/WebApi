using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    /// <summary>
    /// RunFrameBuilder create run frames from a test class.
    /// </summary>
    public class RunFrameBuilder : IRunFrameBuilder
    {
        private ITestClassCommand _testClass;
        private IPerceiverList _perceivers;
        private AbstractRunFrameFactory _runFrameFactory;

        public RunFrameBuilder(ITestClassCommand testClass, IPerceiverList perceivers, AbstractRunFrameFactory runFrameFactory)
        {
            if (testClass == null)
            {
                throw new ArgumentNullException("testClass");
            }

            if (perceivers == null)
            {
                throw new ArgumentNullException("perceivers");
            }

            if (runFrameFactory == null)
            {
                throw new ArgumentNullException("runFrameFactory");
            }

            _perceivers = perceivers;
            _testClass = testClass;
            _runFrameFactory = runFrameFactory;
        }

        public Collection<RunFrame> CreateFrames()
        {
            // perceive the test class and find out all the run elements groups
            RunElementTreeNode[][] elementGroups = _perceivers.Perceivers
                .Select(perceiver => perceiver.Perceive(_testClass)
                                              .Select(element => new RunElementTreeNode(element))
                                              .ToArray())
                .Where(group => group.Length != 0)
                .ToArray();

            // if there is no elements involved, return an empty run frames collection
            // it will cause no actuall test runs
            // TODO: warning the user
            if (!elementGroups.Any())
            {
                return new Collection<RunFrame>();
            }

            // construct tree
            ConstructElementsGraph(elementGroups);

            // traverse the tree and create all run frames
            var stack = new Stack<RunElementTreeNode>();
            var frames = new Collection<RunFrame>();
            ConstructsFrames(elementGroups.First()[0], stack, frames, _runFrameFactory);

            return frames;
        }

        private static void ConstructElementsGraph(RunElementTreeNode[][] elementGroups)
        {
            RunElementTreeNode[] lastGroup = null;
            foreach (var thisGroup in elementGroups)
            {
                for (int i = 1; i < thisGroup.Length; ++i)
                {
                    thisGroup[i - 1].Next = thisGroup[i];
                }

                if (lastGroup != null)
                {
                    foreach (var one in lastGroup)
                    {
                        one.FirstChild = thisGroup[0];
                    }
                }

                lastGroup = thisGroup;
            }
        }

        private static void ConstructsFrames(RunElementTreeNode thisNode, Stack<RunElementTreeNode> stack, Collection<RunFrame> frames, AbstractRunFrameFactory runFrameFactory)
        {
            if (thisNode == null)
            {
                throw new System.ArgumentNullException("thisNode");
            }

            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }

            if (frames == null)
            {
                throw new ArgumentNullException("frames");
            }

            if (runFrameFactory == null)
            {
                throw new ArgumentNullException("runFrameFactory");
            }

            stack.Push(thisNode);

            if (thisNode.FirstChild == null)
            {
                frames.Add(runFrameFactory.CreateFrame(stack.Select(node => node.Element)));
            }
            else
            {
                ConstructsFrames(thisNode.FirstChild, stack, frames, runFrameFactory);
            }

            stack.Pop();
            if (thisNode.Next != null)
            {
                ConstructsFrames(thisNode.Next, stack, frames, runFrameFactory);
            }
        }

        private class RunElementTreeNode
        {
            public RunElementTreeNode(IRunElement element)
            {
                this.Element = element;
            }

            public RunElementTreeNode Next { get; set; }
            public RunElementTreeNode FirstChild { get; set; }
            public IRunElement Element { get; private set; }
        }
    }
}