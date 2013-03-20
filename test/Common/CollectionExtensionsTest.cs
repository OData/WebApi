// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Collections.Generic
{
    public class CollectionExtensionsTest
    {
        [Fact]
        public void SingleDefaultOrErrorIListEmptyReturnsNull()
        {
            IList<object> empty = new List<object>();
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(empty.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListSingleReturns()
        {
            IList<object> single = new List<object>() { new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal(single[0], single.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleDefaultOrErrorIListMultipleThrows()
        {
            IList<object> multiple = new List<object>() { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multiple.SingleDefaultOrError(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListNoMatchReturnsNull()
        {
            IList<object> noMatch = new List<object>() { new object(), new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Null(noMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListOneMatchReturns()
        {
            IList<object> singleMatch = new List<object>() { new object(), "Match", new object() };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                throw new InvalidOperationException();
            };

            Assert.Equal("Match", singleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }

        [Fact]
        public void SingleOfTypeDefaultOrErrorIListMultipleMatchesThrows()
        {
            IList<object> multipleMatch = new List<object>() { new object(), "Match1", new object(), "Match2" };
            object errorArgument = new object();
            Action<object> errorAction = (object argument) =>
            {
                Assert.Equal(errorArgument, argument);
                throw new InvalidOperationException();
            };

            Assert.Throws<InvalidOperationException>(() => multipleMatch.SingleOfTypeDefaultOrError<object, string, object>(errorAction, errorArgument));
        }
    }
}
