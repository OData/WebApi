using Microsoft.AspNet.OData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
{
    public class EdmEntityOperationFactory
    {
        public static IBulkOperation Create(EdmDeltaEntityKind edmDeltaKind, HttpControllerContext httpControllerContext)
        {
            switch (edmDeltaKind)
            {
                case EdmDeltaEntityKind.Entry:
                    return new EdmDeltaEntityOperation(httpControllerContext);
                case EdmDeltaEntityKind.DeletedEntry:
                    return new EdmDeletedEntityOperation(httpControllerContext);
                case EdmDeltaEntityKind.Unknown:
                default:
                    return new EdmEntityOperation(httpControllerContext);
            }

        }
    }
}
