// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using WebStack.QA.Test.OData.SxS.ODataV4.Models;

namespace WebStack.QA.Test.OData.SxS.ODataV4.Controllers
{
    [ODataRoutePrefix("Parts")]
    public class PartsV2Controller : ODataController
    {
        private readonly List<Part> _parts;
        public PartsV2Controller()
        {
            _parts = new List<Part>();
            _parts.AddRange(
                Enumerable.Range(1, 5).Select(n =>
                    new Part()
                    {
                        PartId = n,
                        ReleaseDateTime = DateTimeOffset.Now,
                        Products = Enumerable.Range(1, 3).Select(p => new Product()
                        {
                            Id = p,
                            ManufactureDateTime = DateTimeOffset.Now,
                            Title = string.Concat("Product", p)
                        }).ToList()
                    }));
        }

        // GET odata/Orders
        [EnableQuery]
        [ODataRoute("")]
        public IQueryable<Part> GetAllParts()
        {
            return _parts.AsQueryable();
        }

        // GET odata/Orders(5)
        [EnableQuery]
        [ODataRoute("({key})")]
        public SingleResult<Part> GetOnePart([FromODataUri] int key)
        {
            return SingleResult.Create(_parts.Where(part => part.PartId == key).AsQueryable());
        }
    }
}
