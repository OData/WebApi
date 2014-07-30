// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace System.Web.Http.OData.Formatter
{
    public class PeopleController : ODataController
    {
        [EnableQuery(PageSize = 2)]
        public IEnumerable<FormatterPerson> GetPeople()
        {
            return new FormatterPerson[]
            {
                new FormatterPerson { MyGuid = new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), PerId = 0, Age = 10, Name = "Asha", Order = new FormatterOrder() { OrderName = "FirstOrder", OrderAmount = 235342 }},
                new FormatterPerson { MyGuid = new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902e"), PerId = 1, Age = 11, Name = "Bsha", Order = new FormatterOrder() { OrderName = "SecondOrder", OrderAmount = 123 }},
                new FormatterPerson { MyGuid = new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902f"), PerId = 2, Age = 12, Name = "Csha", Order = new FormatterOrder() { OrderName = "ThirdOrder", OrderAmount = 456 }}
            };
        }

        public FormatterPerson GetFormatterPerson(int key)
        {
            FormatterPerson obj = new FormatterPerson() { MyGuid = new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), PerId = key, Age = 10, Name = "Asha", Order = new FormatterOrder() { OrderName = "FirstOrder", OrderAmount = 235342 } };
            return obj;
        }

        public HttpResponseMessage PostFormatterPerson(FormatterPerson person)
        {
            return Request.CreateResponse(HttpStatusCode.Created, person);
        }
    }
}
