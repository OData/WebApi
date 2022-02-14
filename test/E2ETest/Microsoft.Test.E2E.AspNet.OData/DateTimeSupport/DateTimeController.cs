//-----------------------------------------------------------------------------
// <copyright file="DateTimeController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.DateTimeSupport
{
    public class FilesController : TestODataController
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
                ModifiedDates = new List<DateTime>(){ utcDateTime, localDateTime, unspecifiedDateTime }
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
        public ITestActionResult Get()
        {
            return Ok(_files);
        }

        [EnableQuery]
        public ITestActionResult Get([FromODataUri] int key)
        {
            File file = _files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }

        public ITestActionResult Post([FromBody]File file)
        {
            file.FileId = _files.Count + 1;
            file.ModifiedDates.Add(new DateTime(2014, 12, 24, 9, 2, 3, DateTimeKind.Utc));
            _files.Add(file);
            return Created(file);
        }

        public ITestActionResult Put(int key, [FromBody]File file)
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

        public ITestActionResult Patch(int key, [FromBody]Delta<File> patch)
        {
            File original = _files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            patch.Patch(original);
            return Updated(original);
        }

        public ITestActionResult Delete(int key)
        {
            File original = _files.FirstOrDefault(c => c.FileId == key);
            if (original == null)
            {
                return NotFound();
            }

            _files.Remove(original);
            return StatusCode(HttpStatusCode.NoContent);
        }

        public ITestActionResult GetCreatedDate(int key)
        {
            File file = _files.FirstOrDefault(c => c.FileId == key);
            if (file == null)
            {
                return NotFound();
            }

            return Ok(file.CreatedDate);
        }

        [HttpGet]
        public ITestActionResult GetFilesModifiedAt(DateTimeOffset modifiedDate)
        {
            IEnumerable<File> files = _files.Where(f => f.ModifiedDates.Any(m => m == modifiedDate));
            if (!files.Any())
            {
                return NotFound();
            }

            return Ok(files);
        }

        [HttpPost]
        public ITestActionResult CopyFiles(int key, [FromBody]ODataActionParameters parameters)
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

        [HttpGet]
        public ITestActionResult GetModifiedDates(int key)
        {
            File file = _files.FirstOrDefault(f => f.FileId == key);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file.ModifiedDates);
        }

        [HttpPost]
        public ITestActionResult PostToModifiedDates(int key, [FromBody]DateTime newDateTime)
        {
            File file = _files.FirstOrDefault(f => f.FileId == key);
            if (file == null)
            {
                return NotFound();
            }
            file.ModifiedDates.Add(newDateTime);
            return Updated(file.ModifiedDates);
        }

        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            InitFiles();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
