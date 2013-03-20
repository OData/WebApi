// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    public class ContinuationResult : IHttpActionResult
    {
        private readonly Func<Task<HttpResponseMessage>> _continuation;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Required for compatibility with existing filters")]
        public ContinuationResult(Func<Task<HttpResponseMessage>> continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            _continuation = continuation;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return _continuation();
        }
    }
}
