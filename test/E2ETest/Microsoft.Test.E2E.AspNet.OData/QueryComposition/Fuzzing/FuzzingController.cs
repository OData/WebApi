// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Fuzzing
{
    public class FuzzingController : ApiController
    {
        private static EntityTypeModel1[] cachedEntities = null;

        static FuzzingController()
        {
            cachedEntities = FuzzingDataInitializer.Generate().ToArray();
        }

        [EnableQuery(PageSize = 999999)]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return cachedEntities;
        }
    }

    public class FuzzingDbController : ApiController
    {
        private FuzzingContext context = new FuzzingContext();

        [EnableQuery]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return context.EntityTypeModel1Set.AsEnumerable();
        }

        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }
    }
}
