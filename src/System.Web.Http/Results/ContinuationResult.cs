// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Results
{
    /// <summary>
    /// Represents an action result that returns the results of a specified continuation function.
    /// </summary>
    internal class ContinuationResult : IHttpActionResult
    {
        private readonly Func<Task<HttpResponseMessage>> _continuation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationResult"/> class.
        /// </summary>
        /// <param name="continuation">The continuation function.</param>
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

        /// <summary>Gets the continuation function.</summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Required for compatibility with existing filters")]
        public Func<Task<HttpResponseMessage>> Continuation
        {
            get { return _continuation; }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _continuation();
        }
    }
}
