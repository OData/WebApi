//-----------------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Common
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits until the given task finishes executing and completes in any of the 3 states.
        /// </summary>
        /// <param name="task">A task</param>
        public static void WaitUntilCompleted(this Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            task.ContinueWith(prev =>
            {
                if (prev.IsFaulted)
                {
                    // Observe the exception in the faulted case to avoid an unobserved exception leaking and
                    // killing the thread finalizer.
                    var e = prev.Exception;
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        public static void RethrowFaultedTaskException(this Task task)
        {
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, task.Status);
            throw task.Exception.GetBaseException();
        }
    }
}
