using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Xunit;

namespace WebStack.QA.Test.OData.ComplexTypeInheritance
{
    public class WindowsController : ODataController
    {
        private IList<Window> _windows = new List<Window>();

        public WindowsController()
        {
            Polygon triagle = new Polygon() { HasBorder = true, Vertexes = new List<Point>() };
            triagle.Vertexes.Add(new Point() { X = 1, Y = 2 });
            triagle.Vertexes.Add(new Point() { X = 2, Y = 3 });
            triagle.Vertexes.Add(new Point() { X = 4, Y = 8 });

            Rectangle rectangle = new Rectangle(topLeft: new Point(), width: 2, height: 2);
            Circle circle = new Circle() { HasBorder = true, Center = new Point(), Radius = 2 };

            Window dashboardWindow = new Window
            {
                Id = 1,
                Name = "CircleWindow",
                CurrentShape = circle,
                OptionalShapes = new List<Shape>(),
            };
            dashboardWindow.OptionalShapes.Add(rectangle);
            _windows.Add(dashboardWindow);

            Window popupWindow = new Window
            {
                Id = 2,
                Name = "Popup",
                CurrentShape = rectangle,
                OptionalShapes = new List<Shape>(),
                Parent = dashboardWindow,
            };

            popupWindow.OptionalShapes.Add(triagle);
            popupWindow.OptionalShapes.Add(circle);
            _windows.Add(popupWindow);

            Window anotherPopupWindow = new Window
            {
                Id = 3,
                Name = "AnotherPopup",
                CurrentShape = rectangle,
                OptionalShapes = new List<Shape>(),
                Parent = popupWindow,
            };

            anotherPopupWindow.OptionalShapes.Add(triagle);
            anotherPopupWindow.OptionalShapes.Add(circle);
            _windows.Add(anotherPopupWindow);
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_windows);
        }

        [EnableQuery]
        public SingleResult<Window> GetWindow([FromODataUri] int key)
        {
            return SingleResult.Create<Window>(_windows.Where(w => w.Id == key).AsQueryable());
        }

        public IHttpActionResult Post(Window window)
        {
            _windows.Add(window);
            window.Id = _windows.Count + 1;
            Rectangle rectangle = window.CurrentShape as Rectangle;
            if(rectangle!=null)
            {
                rectangle.Fill();
            }
            window.OptionalShapes.OfType<Rectangle>().ToList().ForEach(r => r.Fill());
            return Created(window);
        }

        [ODataRoute("Windows({key})")]
        public IHttpActionResult Patch(int key, Delta<Window> delta)
        {
            delta.TrySetPropertyValue("Id", key); // It is the key property, and should not be updated.

            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                window = new Window();
                delta.Patch(window);
                return Created(window);
            }

            delta.Patch(window);
            return Ok(window);
        }

        public IHttpActionResult Put(int key, Window window)
        {
            if (key != window.Id)
            {
                return BadRequest();
            }
            Rectangle rectangle = window.CurrentShape as Rectangle;
            if (rectangle != null)
            {
                rectangle.Fill();
            }
            window.OptionalShapes.OfType<Rectangle>().ToList().ForEach(r => r.Fill());

            Window originalWindow = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                _windows.Add(window);
                return Created(window);
            }

            _windows.Remove(originalWindow);
            _windows.Add(window);

            return Ok(window);
        }

        [EnableQuery]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            Window window = _windows.Single(w => w.Id == key);
            _windows.Remove(window);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("Windows({key})/CurrentShape/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle")]
        public IHttpActionResult GetCurrentShape(int key)
        {
            Window window = _windows.FirstOrDefault(w => w.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            Circle circle = window.CurrentShape as Circle;
            if (circle == null)
            {
                return NotFound();
            }
            return Ok(circle);
        }

        [ODataRoute("Windows({key})/CurrentShape/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle/Radius")]
        public IHttpActionResult GetRadius(int key)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            return Ok(((Circle)window.CurrentShape).Radius);
        }

        [ODataRoute("Windows({key})/CurrentShape/HasBorder")]
        public IHttpActionResult GetHasBorder(int key)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            return Ok(window.CurrentShape.HasBorder);
        }

        public IHttpActionResult GetOptionalShapes(int key)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            return Ok(window.OptionalShapes);
        }

        // https://github.com/OData/odata.net/issues/457: [UriParser] Cast segment following a collection complex type property reports exception.
        // [ODataRoute("Windows({key})/OptionalShapes/WebStack.QA.Test.OData.ComplexTypeInheritance.Circle")]
        public IHttpActionResult GetOptionalShapesOfCircle(int key)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            return Ok(window.OptionalShapes.OfType<Circle>());
        }

        [HttpPut]
     //   [ODataRoute("Windows({key})/CurrentShape")]
        public IHttpActionResult PutToCurrentShapeOfCircle(int key, Delta<Circle> shape)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            Circle origin = window.CurrentShape as Circle;
            if (origin == null)
            {
                return NotFound();
            }

            shape.Put(origin);
            return Ok(origin);
        }

        [HttpPut]
        [ODataRoute("Windows({key})/OptionalShapes")]
        public IHttpActionResult ReplaceOptionalShapes(int key, IEnumerable<Shape> shapes)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            Assert.NotNull(shapes);
            window.OptionalShapes = shapes.ToList();
            return Ok(shapes);
        }

        [HttpPatch]
        public IHttpActionResult PatchToOptionalShapes(int key, Delta<Shape> shapes)
        {
            return Ok("Not Supported");
        }

        [HttpPatch]
        public IHttpActionResult PatchToCurrentShapeOfCircle(int key, Delta<Circle> shape)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            Circle origin = window.CurrentShape as Circle;
            if (origin == null)
            {
                return NotFound();
            }

            shape.Patch(origin);
            return Ok(origin);
        }

        public IHttpActionResult DeleteToCurrentShape(int key)
        {
            Window window = _windows.FirstOrDefault(e => e.Id == key);
            if (window == null)
            {
                return NotFound();
            }

            window.CurrentShape = null;
            return Updated(window);
        }
    }
}
