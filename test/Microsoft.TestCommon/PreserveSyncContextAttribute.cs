// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using Xunit;

namespace Microsoft.TestCommon
{
    /// <summary>
    /// Preserves the current <see cref="SynchronizationContext"/>. Use this attribute on
    /// tests which modify the current <see cref="SynchronizationContext"/>.
    /// </summary>
    public class PreserveSyncContextAttribute : BeforeAfterTestAttribute
    {
        private SynchronizationContext _syncContext;

        public override void Before(System.Reflection.MethodInfo methodUnderTest)
        {
            _syncContext = SynchronizationContext.Current;
        }

        public override void After(System.Reflection.MethodInfo methodUnderTest)
        {
            SynchronizationContext.SetSynchronizationContext(_syncContext);
        }
    }
}
