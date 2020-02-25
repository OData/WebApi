// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DollarId
{
    public class SingersController : TestODataController
    {
        // #0  For DollarIdClientTest
        // #1  For DollarIdTest
        public static List<Singer> Singers = Enumerable.Range(0, 2).Select(i =>
            new Singer()
            {
                ID = i,
                Name = string.Format("Name {0}", i),
                Albums = new List<Album>
                {
                    new Album { ID = 0 },
                    new Album { ID = 1 },
                    new Album { ID = 2 }
                }
            }).ToList();

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

        #endregion
    }

    public class AlbumsController : TestODataController
    {
        // #0  For DollarIdClientTest
        // #1  For DollarIdTest
        public static List<Album> Albums = Enumerable.Range(0, 2).Select(i =>
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
            Assert.Equal(3, key); // 3 is a magic test value from test case.

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
    }
}
