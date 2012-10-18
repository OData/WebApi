// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Mvc.Facebook.Extensions;
using Microsoft.AspNet.Mvc.Facebook.Models;
using Microsoft.AspNet.Mvc.Facebook.Services;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Mvc.Facebook.Controllers
{
    public class FacebookRealtimeController : ApiController
    {
        private readonly IFacebookService facebookService;
        private readonly IFacebookUserStorageService facebookUserStorageService;

        public FacebookRealtimeController()
            : this(DefaultFacebookService.Instance, FacebookSettings.DefaultUserStorageService)
        {
        }

        public FacebookRealtimeController(IFacebookService facebookService, IFacebookUserStorageService facebookUserStorageService)
        {
            this.facebookService = facebookService;
            this.facebookUserStorageService = facebookUserStorageService;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public HttpResponseMessage Get([FromUri(Name = "hub")] SubscriptionVerification subscriptionVerification)
        {
            if (subscriptionVerification.Mode == "subscribe" && subscriptionVerification.Verify_Token == facebookService.VerificationToken)
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(subscriptionVerification.Challenge);
                return response;
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        //TODO: (ErikPo) Make this async
        public HttpResponseMessage Post(JObject facebookObject)
        {
#if Debug
            Utilities.Log(facebookObject.ToString());
#endif
            //TODO: (ErikPo) Find out for sure if we need to validate the request here during the security review

            //TODO: (ErikPo) This code needs to move into some sort of user change handler
            if (facebookObject["object"].Value<string>() == "user")
            {
                foreach (var entry in facebookObject["entry"])
                {
                    var facebookId = entry["id"].Value<string>();
                    var fields = new List<string>();
                    foreach (var changedField in entry["changed_fields"])
                    {
                        fields.Add(changedField.Value<string>());
                    }
                    if (fields.Count > 0)
                    {
                        var user = facebookUserStorageService.GetUser(facebookId);
                        if (user != null)
                        {
                            facebookService.RefreshUserFields(user, fields.ToArray());
                            FacebookUserEvents.FireUserChangedEvent(user);
                        }
                    }
                }
            }

            //TODO: (ErikPo) Parse other stuff like friend changes

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
