// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Json
{
    public static class ExceptionHelper
    {
        public static void Throws<TException>(Assert.ThrowsDelegate act, Action<TException> exceptionAssert) where TException : Exception
        {
            Exception ex = Record.Exception(act);
            Assert.NotNull(ex);
            TException tex = Assert.IsAssignableFrom<TException>(ex);
            exceptionAssert(tex);
        }

        public static void Throws<TException>(Assert.ThrowsDelegate act, string message) where TException : Exception
        {
            Throws<TException>(act, ex => Assert.Equal(message, ex.Message));
        }

        public static void Throws<TException>(Assert.ThrowsDelegate act) where TException : Exception
        {
            Throws<TException>(act, _ => { });
        }


    }
}
