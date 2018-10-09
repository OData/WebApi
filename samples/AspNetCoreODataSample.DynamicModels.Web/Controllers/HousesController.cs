using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.DynamicModels.Web.Controllers
{
    public class HousesController : ODataController
    {
        private readonly HouseContext _context;

        public HousesController(HouseContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Houses);
        }

        [EnableQuery]
        public ActionResult Get(int key)
        {
            var item = _context.Houses.SingleOrDefault(p => p.ID == key);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // GET /House(1)/Rooms
        [EnableQuery]
        public IQueryable<Room> GetRooms([FromODataUri] int key)
        {
            return _context.Houses.Where(i => i.ID == key).SelectMany(i => i.Rooms);
        }

    }
}
