// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Extensions;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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

        public HttpResponseMessage GetProperty(int key, string property)
        {
            var entity = LocalTable[key];
            object propertyValue = entity.GetType().GetProperty(property).GetValue(entity, null);
            var result = Request.CreateResponse(HttpStatusCode.OK, propertyValue, entity.GetType().GetProperty(property).PropertyType);
            return result;
        }
    }
}
