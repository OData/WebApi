//-----------------------------------------------------------------------------
// <copyright file="PresidentController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Net;
using Microsoft.AspNet.OData.Test.Extensions;
#else
using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNet.OData;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
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

#if NETCORE
        public AspNetCore.Http.HttpResponse Post(FormatterPerson person)
        {
            return Request.CreateResponse(HttpStatusCode.Created, person);
        }
#else
        public System.Net.Http.HttpResponseMessage Post(FormatterPerson person)
        {
            return Request.CreateResponse(HttpStatusCode.Created, person);
        }
#endif
    }
}
