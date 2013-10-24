// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace System.Web.Http.Owin.ExceptionHandling
{
    internal class EmptyExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            return TaskHelpers.Completed();
        }
    }
}
