//-----------------------------------------------------------------------------
// <copyright file="TaskHelpersExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Common
{
    internal static class TaskHelpersExtensions
    {
        /// <summary>
        /// Cast Task to Task of object
        /// </summary>
        internal static async Task<object> CastToObject(this Task task)
        {
            await task;
            return null;
        }

        /// <summary>
        /// Cast Task of T to Task of object
        /// </summary>
        internal static async Task<object> CastToObject<T>(this Task<T> task)
        {
            return (object)await task;
        }

        /// <summary>
        /// Throws the first faulting exception for a task which is faulted. It preserves the original stack trace when
        /// throwing the exception. Note: It is the caller's responsibility not to pass incomplete tasks to this
        /// method, because it does degenerate into a call to the equivalent of .Wait() on the task when it hasn't yet
        /// completed.
        /// </summary>
        internal static void ThrowIfFaulted(this Task task)
        {
            task.GetAwaiter().GetResult();
        }
    }
}
