// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter
{
    public class PeopleController : ApiController
    {
        public FormatterPerson GetPerson(int id)
        {
            FormatterPerson obj = new FormatterPerson() { MyGuid = new Guid("f99080c0-2f9e-472e-8c72-1a8ecd9f902d"), PerId = id, Age = 10, Name = "Asha", Order = new FormatterOrder() { OrderName = "FirstOrder", OrderAmount = 235342 } };
            return obj;
        }
    }
}
