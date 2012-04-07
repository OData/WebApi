// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Threading;
using Xunit;

namespace System.Web.TestUtil
{
    public class CultureReplacer : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly long _threadId;

        public CultureReplacer(string culture = "en-us")
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Assert.True(Thread.CurrentThread.ManagedThreadId == _threadId, "The current thread is not the same as the thread invoking the constructor. This should never happen.");
                Thread.CurrentThread.CurrentCulture = _originalCulture;
            }
        }
    }
}
