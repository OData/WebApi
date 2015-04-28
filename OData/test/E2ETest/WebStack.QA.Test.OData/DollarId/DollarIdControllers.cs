using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.DollarId
{
    public class SingersController : ODataController
    {
        public static List<Singer> Singers;

        static SingersController()
        {
            InitData();
        }

        private static void InitData()
        {
            Singers = Enumerable.Range(0, 5).Select(i =>
                   new Singer()
                   {
                       ID = i,
                       Name = string.Format("Name {0}", i)
                   }).ToList();
            var singer = Singers.Single(s => s.ID == 0);
            singer.Albums = new List<Album>();
            singer.Albums.AddRange(AlbumsController.Albums.Take(3));
        }

        #region Actions

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Singers);
        }

        public IHttpActionResult Get(int key)
        {
            return Ok(Singers.Single(s => s.ID == key));
        }

        public IHttpActionResult GetAlbumsFromSinger(int key)
        {
            var singer = Singers.Single(s => s.ID == key);
            return Ok(singer.Albums);
        }

        public IHttpActionResult DeleteRef(int key, int relatedKey, string navigationProperty)
        {
            var singer = Singers.Single(s => s.ID == key);
            var album = singer.Albums.Single(a => a.ID == relatedKey);

            if (navigationProperty != "Albums")
            {
                return BadRequest();
            }

            singer.Albums.Remove(album);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("Singers/WebStack.QA.Test.OData.DollarId.ResetDataSource")]
        public IHttpActionResult ResetDataSourceOnCollectionOfSinger()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion
    }

    public class AlbumsController : ODataController
    {
        public static List<Album> Albums;

        static AlbumsController()
        {
            InitData();
        }

        private static void InitData()
        {
            Albums = Enumerable.Range(0, 10).Select(i =>
                   new Album()
                   {
                       ID = i,
                       Name = string.Format("Name {0}", i),
                       Sales = new List<AreaSales>()
                           {
                               new AreaSales()
                                   {
                                       ID = 1 + i,
                                       City = string.Format("City{0}", i),
                                       Sales = 1000 * i
                                   },
                               new AreaSales()
                                   {
                                       ID = 2 + i,
                                       City = string.Format("City{0}", i),
                                       Sales = 2 * 1000 * i
                                   }
                           }
                   }).ToList();
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Albums);
        }

        public IHttpActionResult Get(int key)
        {
            return Ok(Albums.Single(s => s.ID == key));
        }

        // ~/Albums({key})/WebStack.QA.Test.OData.DollarId.GetSinger()"
        [HttpGet]
        [EnableQuery]
        public IHttpActionResult GetSingers(int key)
        {
            if (Albums.SingleOrDefault(s => s.ID == key) == null)
            {
                return BadRequest();
            }
            IList<Singer> singers = new List<Singer>();
            singers.Add(new Singer()
            {
                ID = 101,
                Name = string.Format("Name101"),
                MasterPiece="abc",
            });
            singers.Add(new Singer()
            {
                ID = 102,
                Name = string.Format("Name102"),
                MasterPiece="def",
            });
            return Ok(singers);
        }

        public IHttpActionResult GetSalesFromAlbum(int key)
        {
            var album = Albums.Single(a => a.ID == key);
            return Ok(album.Sales);
        }

        [HttpDelete]
        [ODataRoute("Albums({key})/Sales({relatedKey})/$ref")]
        public IHttpActionResult DeleteSalesInfoFromAlum(int key, int relatedKey)
        {
            var album = Albums.Single(a => a.ID == key);
            var sales = album.Sales.Single(s => s.ID == relatedKey);

            if (album.Sales.Remove(sales))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ODataRoute("Albums/WebStack.QA.Test.OData.DollarId.ResetDataSource")]
        public IHttpActionResult ResetDataSourceOnCollectionOfAlbum()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
