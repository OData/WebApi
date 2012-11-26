// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Realtime
{
    public abstract class FacebookRealtimeUpdateController : ApiController
    {
        private static readonly string XHubSignatureHeaderName = "X-Hub-Signature";
        private FacebookConfiguration _facebookConfiguration;

        public abstract string VerifyToken { get; }

        public FacebookConfiguration FacebookConfiguration
        {
            get
            {
                if (_facebookConfiguration == null)
                {
                    _facebookConfiguration = GlobalFacebookConfiguration.Configuration;
                }
                return _facebookConfiguration;
            }
            set
            {
                _facebookConfiguration = value;
            }
        }

        [NonAction]
        public abstract Task HandleUpdateAsync(ChangeNotification notification);

        public virtual HttpResponseMessage Get(SubscriptionVerification subscriptionVerification)
        {
            try
            {
                FacebookClient client = FacebookConfiguration.ClientProvider.CreateClient();
                client.VerifyGetSubscription(subscriptionVerification.Mode, subscriptionVerification.Verify_Token, subscriptionVerification.Challenge, VerifyToken);
            }
            catch (ArgumentException argumentException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, argumentException);
            }

            return new HttpResponseMessage
            {
                Content = new StringContent(subscriptionVerification.Challenge)
            };
        }

        public virtual async Task<HttpResponseMessage> Post()
        {
            IEnumerable<string> headerValues;
            if (Request.Headers.TryGetValues(XHubSignatureHeaderName, out headerValues))
            {
                string signatureHeaderValue = headerValues.FirstOrDefault();
                try
                {
                    string contentString = await Request.Content.ReadAsStringAsync();
                    FacebookClient client = FacebookConfiguration.ClientProvider.CreateClient();
                    ChangeNotification notification = client.VerifyPostSubscription(signatureHeaderValue, contentString, typeof(ChangeNotification)) as ChangeNotification;
                    await HandleUpdateAsync(notification);
                }
                catch (ArgumentException argumentException)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, argumentException);
                }
                catch (HttpResponseException responseException)
                {
                    return responseException.Response;
                }
            }
            else
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    String.Format(CultureInfo.CurrentCulture, Resources.MissingRequiredHeader, XHubSignatureHeaderName));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
