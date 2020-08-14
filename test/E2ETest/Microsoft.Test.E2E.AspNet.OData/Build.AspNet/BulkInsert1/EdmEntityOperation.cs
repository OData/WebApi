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
    public class EdmEntityOperation : IBulkOperation
    {
        BulkOperationHelper bulkOperationHelper;
        IDictionary<string, object> argDict;

        public EdmEntityOperation(HttpControllerContext controllerContext)
        {
            bulkOperationHelper = new BulkOperationHelper(controllerContext, "Post");
        }

        public virtual IEdmStructuredObject ApplyEntityOperation<T>(IEdmStructuredObject edmEntityObject, T objectType)
        {            
            var edmEntity = edmEntityObject as EdmEntityObject;

            var instance = (T) Activator.CreateInstance(typeof(T)) ;

            if (edmEntity != null)
            {
                foreach(var prop in typeof(T).GetProperties() )
                {
                    object val;
                    edmEntityObject.TryGetPropertyValue(prop.Name, out val);
                    prop.SetValue(instance, val);
                }

            }

          
            argDict = new Dictionary<string, object>();
            argDict.Add("parameter", instance);

            var response = bulkOperationHelper.ApplyOperation(argDict);
           
            var edmEntityRes = JsonConvert.DeserializeObject(response, typeof(EdmDeltaEntityObject)) as EdmDeltaEntityObject;


            return edmEntityRes;
        }
    }
}
