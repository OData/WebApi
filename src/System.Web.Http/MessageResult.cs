// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http
{
    /// <summary>Represents an action result that returns a specified response message.</summary>
    public class MessageResult : IHttpActionResult
    {
        private readonly HttpResponseMessage _response;

        /// <summary>Initializes a new instance of the <see cref="MessageResult"/> class.</summary>
        /// <param name="response">The response message.</param>
        public MessageResult(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            _response = response;
        }

        /// <summary>Gets the response message.</summary>
        public HttpResponseMessage Response
        {
            get { return _response; }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
