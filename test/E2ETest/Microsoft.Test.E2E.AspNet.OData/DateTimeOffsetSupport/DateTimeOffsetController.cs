//-----------------------------------------------------------------------------
// <copyright file="DateTimeOffsetController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeOffsetSupport
{
    public class FilesController : ODataController
    {
        private static readonly FilesContext _db = new FilesContext();

        public static List<File> CreateFiles(DateTimeOffset dateTime)
        {
            return Enumerable.Range(1, 5).Select(e =>
            new File
            {
                FileId = e,
                Name = "File #" + e,
                CreatedDate = dateTime.AddMonths(3 - e),
                DeleteDate = dateTime,
            }).ToList();
        }

        public FilesController() { }

        [EnableQuery]
        public IQueryable<File> Get()
        {
            var db = new FilesContext();
            return db.Files;
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var db = new FilesContext();
            File file = db.Files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }

        public IHttpActionResult Post(File file)
        {
            var db = new FilesContext();
            db.Files.Add(file);
            db.SaveChanges();

            return Created(file);
        }

        public IHttpActionResult Patch(int key, Delta<File> patch)
        {
            var db = new FilesContext();
            var file = db.Files.SingleOrDefault(c => c.FileId == key);

            if (file == null)
            {
                return NotFound();
            }

            patch.Patch(file);
            db.SaveChanges();

            return Updated(file);
        }

        public IHttpActionResult Delete(int key)
        {
            var db = new FilesContext();
            File original = db.Files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            db.Files.Remove(original);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult GetCreatedDate(int key)
        {
            var db = new FilesContext();
            File file = db.Files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            return Ok(file.CreatedDate);
        }

        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource(String time)
        {
            var dateTime = DateTimeOffset.Parse(time.Replace(' ', '+'));
            var files = CreateFiles(dateTime);

            _db.Files.RemoveRange(_db.Files);
            _db.SaveChanges();

            _db.Database.Delete();
            _db.Database.Create();

            _db.Files.AddRange(files);
            _db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
