using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Deserialization;
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
    public class EdmDeltaEntityOperation: IBulkOperation
    {
        BulkOperationHelper bulkOperationHelper;
        IDictionary<string, object> argDict;

        public EdmDeltaEntityOperation(HttpControllerContext controllerContext)
        {
            bulkOperationHelper = new BulkOperationHelper(controllerContext,"Patch");
        }


        public virtual IEdmStructuredObject ApplyEntityOperation<T>(IEdmStructuredObject edmEntityObject, T objectType)
        {
            var data = new ExpandoObject() as IDictionary<string, object>;

            var changedEntity = edmEntityObject as EdmDeltaEntityObject;

            if (changedEntity != null)
            {
                foreach (var prop in changedEntity.GetChangedPropertyNames())
                {
                    object val;
                    if (changedEntity.TryGetPropertyValue(prop, out val))
                    {
                        data.Add(prop, val);
                    }

                }
            }
            argDict = new Dictionary<string, object>();
            argDict.Add("parameter", data);

            var response = bulkOperationHelper.ApplyOperation(argDict);
            var deltaEntity = JsonConvert.DeserializeObject(response, typeof(EdmDeltaEntityObject) ) as EdmDeltaEntityObject;


            return deltaEntity;
        }
    }
}
