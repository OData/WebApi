using System.Diagnostics;
using System.Linq;
using AspNetCoreODataSample.DynamicModels.Web.Edm;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace AspNetCoreODataSample.DynamicModels.Web.Controllers
{
    public class InteriorController : ODataController
    {
        private readonly HouseContext _context;

        public InteriorController(HouseContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(GetInterior());
        }

        [EnableQuery]
        public SingleResult<Interior> Get([FromODataUri] int key)
        {
            return SingleResult.Create(GetInterior().Where(p => p.ID == key));
        }

        // GET /Interior(1)/Room
        [EnableQuery]
        public SingleResult<Room> GetRoom([FromODataUri] int key)
        {
            return SingleResult.Create(GetInterior().Where(i => i.ID == key).Select(i => i.Room));
        }

        public IActionResult Post([FromBody] Interior entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (entity.DefinitionID == 0)
            {
                ModelState.AddModelError<Interior>(e => e.Definition, "Could not determine type of interior, please specify the type.");
                return BadRequest(ModelState);
            }

            entity.ID = 0;
            _context.Add(entity);
            _context.SaveChanges();
            return Created(entity);
        }

        public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Interior> entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = GetInterior().SingleOrDefault(i => i.ID == key);
            if (existing == null)
            {
                return NotFound();
            }
            entity.Patch(existing);
            _context.SaveChanges();
            return Updated(entity);
        }

        public IActionResult Put([FromODataUri] int key, [FromBody] Interior entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (entity.DefinitionID == 0)
            {
                ModelState.AddModelError<Interior>(e => e.Definition, "Could not determine type of interior, please specify the type.");
                return BadRequest(ModelState);
            }

            if (entity.ID == 0)
            {
                entity.ID = key;
            }

            if (key != entity.ID)
            {
                return BadRequest();
            }

            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
            return Updated(entity);
        }

        public IActionResult Delete([FromODataUri] int key)
        {
            var interior = GetInterior().SingleOrDefault(i => i.ID == key);
            if (interior == null)
            {
                return NotFound();
            }
            _context.Interior.Remove(interior);
            _context.SaveChanges();
            return NoContent();
        }

        private IQueryable<Interior> GetInterior()
        {
            var odataPath = HttpContext.ODataFeature().Path;
            if (!(odataPath.Segments.FirstOrDefault() is EntitySetSegment entitySetSegment))
            {
                Debug.Fail("We should only come into this controller with a proper entity segment");
                return null;
            }

            // unwrap which entity was requested
            var model = Request.GetModel();
            var collectionType = ((IEdmCollectionType)entitySetSegment.EntitySet.Type);
            var elementType = (IEdmSchemaElement)collectionType.ElementType.Definition;

            IQueryable<Interior> interiorQuery = _context.Interior
                .Include(i => i.Definition);

            // filter for specific interior type if needed
            var interiorDefinition = model.GetAnnotationValue<InteriorDefinitionAnnotation>(elementType);
            if (interiorDefinition != null)
            {
                interiorQuery = interiorQuery.Where(e => e.Definition.ID == interiorDefinition.DefinitionID);
            }

            // wrap to correct collection type
            return interiorQuery.WithEdmCollectionType(collectionType);
        }
    }
}
