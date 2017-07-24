﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using WebStack.QA.Test.OData.SxS.ODataV3.Models;

namespace WebStack.QA.Test.OData.SxS.ODataV3.Controllers
{
    public class PartsController : ODataController
    {
        private readonly List<Part> _parts;
        public PartsController()
        {
            _parts = new List<Part>();
            _parts.AddRange(
                Enumerable.Range(1, 5).Select(n =>
                    new Part()
                    {
                        PartId = n,
                        ReleaseDateTime = DateTime.Now,
                        Products = Enumerable.Range(1, 3).Select(p => new Product()
                        {
                            Id = p,
                            ManufactureDateTime = DateTime.Now,
                            Title = string.Concat("Product", p)
                        }).ToList()
                    }));
        }

        // GET odata/Orders
        [EnableQuery]
        public IQueryable<Part> GetParts()
        {
            return _parts.AsQueryable();
        }

        // GET odata/Orders(5)
        [EnableQuery]
        public SingleResult<Part> GetPart([FromODataUri] int key)
        {
            return SingleResult.Create(_parts.Where(part => part.PartId == key).AsQueryable());
        }
    }
}
