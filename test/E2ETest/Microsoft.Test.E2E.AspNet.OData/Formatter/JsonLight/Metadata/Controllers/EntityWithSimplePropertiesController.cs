//-----------------------------------------------------------------------------
// <copyright file="EntityWithSimplePropertiesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class EntityWithSimplePropertiesController : InMemoryODataController<EntityWithSimpleProperties, int>
    {
        public EntityWithSimplePropertiesController()
            : base("Id")
        {
            EntityWithSimpleProperties[] entities = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>();
            foreach (var entity in entities)
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }

#if NETCORE
        public ITestActionResult GetProperty(int key, string property)
        {
            var entity = LocalTable[key];
            object propertyValue = entity.GetType().GetProperty(property).GetValue(entity, null);
            var result = Ok(propertyValue);
            return result;
        }
#else
        public HttpResponseMessage GetProperty(int key, string property)
        {
            var entity = LocalTable[key];
            object propertyValue = entity.GetType().GetProperty(property).GetValue(entity, null);
            var result = Request.CreateResponse(HttpStatusCode.OK, propertyValue, entity.GetType().GetProperty(property).PropertyType);
            return result;
        }
#endif
    }
}
