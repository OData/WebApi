using System.Diagnostics;
using System.Linq;
using AspNetCoreODataSample.DynamicModels.Web.Edm;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult Get(int key)
        {
            var item = GetInterior().SingleOrDefault(p => p.ID == key);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
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
