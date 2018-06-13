// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using AspNetODataSample.Web.Models;
using Microsoft.AspNet.OData;

namespace AspNetODataSample.Web.Controllers
{
    public class TodoItemsController : ODataController
    {
        private TodoItemContext _db = new TodoItemContext();

        public TodoItemsController()
        {
            if (!_db.TodoItems.Any())
            {
                foreach (var a in DataSource.GetTodoItems())
                {
                    _db.TodoItems.Add(a);
                }

                _db.SaveChanges();
            }
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_db.TodoItems);
        }

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            return Ok(_db.TodoItems.FirstOrDefault(c => c.Id == key));
        }

        [HttpPost]
        public IHttpActionResult Post(TodoItem item)
        {
            _db.TodoItems.Add(item);
            _db.SaveChanges();
            return Created(item);
        }
    }
}