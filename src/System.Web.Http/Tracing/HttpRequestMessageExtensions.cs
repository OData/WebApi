using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http.Tracing
{
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Retrieves the <see cref="Guid"/> which has been assigned as the
        /// correlation id associated with the given <paramref name="request"/>.
        /// The value will be created and set the first time this method is called.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>The <see cref="Guid"/> associated with that request.</returns>
        public static Guid GetCorrelationId(this HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            Guid correlationId;
            if (!request.Properties.TryGetValue<Guid>(HttpPropertyKeys.RequestCorrelationKey, out correlationId))
            {
                correlationId = Guid.NewGuid();
                request.Properties.Add(HttpPropertyKeys.RequestCorrelationKey, correlationId);
            }

            return correlationId;
        }
    }
}
