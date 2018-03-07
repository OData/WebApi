// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.DollarId
{
    public class SingersController : TestODataController
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
        public ITestActionResult Get()
        {
            return Ok(Singers);
        }

        public ITestActionResult Get(int key)
        {
            return Ok(Singers.Single(s => s.ID == key));
        }

        public ITestActionResult GetAlbumsFromSinger(int key)
        {
            var singer = Singers.Single(s => s.ID == key);
            return Ok(singer.Albums);
        }

        public ITestActionResult DeleteRef(int key, int relatedKey, string navigationProperty)
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
        [ODataRoute("Singers/Microsoft.Test.E2E.AspNet.OData.DollarId.ResetDataSource")]
        public ITestActionResult ResetDataSourceOnCollectionOfSinger()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion
    }

    public class AlbumsController : TestODataController
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
        public ITestActionResult Get()
        {
            return Ok(Albums);
        }

        public ITestActionResult Get(int key)
        {
            return Ok(Albums.Single(s => s.ID == key));
        }

        // ~/Albums({key})/Microsoft.Test.E2E.AspNet.OData.DollarId.GetSinger()"
        [HttpGet]
        [EnableQuery]
        public ITestActionResult GetSingers(int key)
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

        public ITestActionResult GetSalesFromAlbum(int key)
        {
            var album = Albums.Single(a => a.ID == key);
            return Ok(album.Sales);
        }

        [HttpDelete]
        [ODataRoute("Albums({key})/Sales({relatedKey})/$ref")]
        public ITestActionResult DeleteSalesInfoFromAlum(int key, int relatedKey)
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
        [ODataRoute("Albums/Microsoft.Test.E2E.AspNet.OData.DollarId.ResetDataSource")]
        public ITestActionResult ResetDataSourceOnCollectionOfAlbum()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
