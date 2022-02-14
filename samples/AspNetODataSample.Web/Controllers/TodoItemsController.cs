//-----------------------------------------------------------------------------
// <copyright file="TodoItemsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
