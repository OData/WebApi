// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace Microsoft.AspNet.Mvc.Facebook.Models
{
    [FromUri(Name = "hub")]
    public class SubscriptionVerification
    {
        public string Mode { get; set; }
        public string Verify_Token { get; set; }
        public string Challenge { get; set; }
    }
}
