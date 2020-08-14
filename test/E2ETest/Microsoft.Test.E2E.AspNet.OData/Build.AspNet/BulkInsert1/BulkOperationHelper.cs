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
    public class BulkOperationHelper 
    {
        ReflectedHttpActionDescriptor actionDescriptor;
        HttpControllerContext controllerContext;

        public BulkOperationHelper(HttpControllerContext controllerContext, string httpVerb)
        {
            this.controllerContext = controllerContext;
            actionDescriptor = new ReflectedHttpActionDescriptor(controllerContext.ControllerDescriptor, controllerContext.ControllerDescriptor.ControllerType.GetMethod(httpVerb));
        }

        public string ApplyOperation(IDictionary<string,object> actionArgDict)
        {
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
           
            foreach(var kv in actionArgDict)
            {
                actionContext.ActionArguments.Add(kv.Key, kv.Value);
            }           

            var apiActionInvoker = new ApiControllerActionInvoke();
            var response = apiActionInvoker.InvokeActionAsync(actionContext, CancellationToken.None);

            return response.Result.Content.ReadAsStringAsync().Result;
        }
    }
}
