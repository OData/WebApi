// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeSupport
{
    public class FilesController : ODataController
    {
        private static IList<File> _files;

        private static void InitFiles()
        {
            DateTime utcDateTime = new DateTime(2014, 12, 24, 1, 2, 3, DateTimeKind.Utc);
            DateTime localDateTime = new DateTime(2014, 12, 24, 1, 2, 3, DateTimeKind.Local);
            DateTime unspecifiedDateTime = new DateTime(2014, 12, 24, 1, 2, 3, DateTimeKind.Unspecified);
            _files = Enumerable.Range(1, 5).Select(e =>
            new File
            {
                FileId = e,
                Name = "File #" + e,
                CreatedDate = utcDateTime.AddYears(e),
                DeleteDate = e % 2 == 0 ? (DateTime?)null : localDateTime.AddHours(e * 5),
                ModifiedDates = new []{ utcDateTime, localDateTime, unspecifiedDateTime }
            }).ToList();
        }

        public FilesController()
        {
            if (_files == null)
            {
                InitFiles();
            }
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_files);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            File file = _files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }

        public IHttpActionResult Post(File file)
        {
            file.FileId = _files.Count + 1;
            file.ModifiedDates.Add(new DateTime(2014, 12, 24, 9, 2, 3, DateTimeKind.Utc));
            _files.Add(file);
            return Created(file);
        }

        public IHttpActionResult Put(int key, File file)
        {
            if (key != file.FileId)
            {
                return BadRequest("The FileId of file is not matched with the key");
            }

            File original = _files.FirstOrDefault(c => c.FileId == key);
            _files.Remove(original);
            _files.Add(file);
            return Updated(file);
        }

        public IHttpActionResult Patch(int key, Delta<File> patch)
        {
            File original = _files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            patch.Patch(original);
            return Updated(original);
        }

        public IHttpActionResult Delete(int key)
        {
            File original = _files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            _files.Remove(original);
            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult GetCreatedDate(int key)
        {
            File file = _files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            return Ok(file.CreatedDate);
        }

        [HttpGet]
        public IHttpActionResult GetFilesModifiedAt(DateTimeOffset modifiedDate)
        {
            IEnumerable<File> files = _files.Where(f => f.ModifiedDates.Any(m => m == modifiedDate));
            if (!files.Any())
            {
                return NotFound();
            }

            return Ok(files);
        }

        [HttpPost]
        public IHttpActionResult CopyFiles(int key, ODataActionParameters parameters)
        {
            object value;
            if (!parameters.TryGetValue("createdDate", out value))
            {
                return NotFound();
            }

            DateTimeOffset createdDate = (DateTimeOffset)value;
            createdDate.AddYears(1).AddHours(9);

            return Ok(createdDate);
        }

        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource()
        {
            InitFiles();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
