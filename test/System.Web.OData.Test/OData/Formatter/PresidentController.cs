// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;

namespace System.Web.OData.Formatter
{
    public class PresidentController : ODataController
    {
        private FormatterPerson president = new FormatterPerson
        {
            MyGuid = new Guid("0FFFEF2B-E5DE-4B7C-B943-B1F7DA006FCD"),
            PerId = 1,
            Age = 52,
            Name = "Barack Obama",
            Order = new FormatterOrder() 
            {
                OrderName = "US Order",
                OrderAmount = 12345
            }
         };

        public FormatterPerson Get()
        {
            return president;
        }

        public HttpResponseMessage Post(FormatterPerson person)
        {
            return Request.CreateResponse(HttpStatusCode.Created, person);
        }
    }
}
