using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
{
    public class EdmDeletedEntityOperation : IBulkOperation
    {
        BulkOperationHelper bulkOperationHelper;
        IDictionary<string, object> argDict;

        public EdmDeletedEntityOperation(HttpControllerContext controllerContext)
        {
            bulkOperationHelper = new BulkOperationHelper(controllerContext,"Delete");
        }

        public virtual IEdmStructuredObject ApplyEntityOperation<T>(IEdmStructuredObject edmEntityObject, T objectType)
        {
            var edmDeletedEntity = edmEntityObject as EdmDeltaDeletedEntityObject;

            if (edmDeletedEntity != null)
            {
                argDict = new Dictionary<string, object>();
                argDict.Add("key", edmDeletedEntity.Id);

                var response = bulkOperationHelper.ApplyOperation(argDict);

                JsonConvert.DeserializeObject(response, typeof(EdmDeltaDeletedEntityObject));

            }
            return edmDeletedEntity;
        }
    }
}
