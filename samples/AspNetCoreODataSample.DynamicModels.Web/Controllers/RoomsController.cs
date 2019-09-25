using System.Linq;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.DynamicModels.Web.Controllers
{
    public class RoomsController : ODataController
    {
        private readonly HouseContext _context;

        public RoomsController(HouseContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Rooms);
        }

        [EnableQuery]
        public ActionResult Get(int key)
        {
            var item = _context.Rooms.SingleOrDefault(p => p.ID == key);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // GET /Rooms(1)/Interior
        [EnableQuery]
        public IQueryable<Interior> GetInterior([FromODataUri] int key)
        {
            return _context.Rooms.Where(i => i.ID == key).SelectMany(i => i.Interior);
        }
    }
}
