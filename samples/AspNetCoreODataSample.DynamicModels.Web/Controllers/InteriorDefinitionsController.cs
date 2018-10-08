using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreODataSample.DynamicModels.Web.Controllers
{
    public class InteriorDefinitionsController : ODataController
    {
        private readonly HouseContext _context;

        public InteriorDefinitionsController(HouseContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.InteriorDefinitions);
        }

        [EnableQuery]
        public ActionResult Get(int key)
        {
            var item = _context.InteriorDefinitions
                .Include(d => d.Properties)
                .SingleOrDefault(p => p.ID == key);

            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}
