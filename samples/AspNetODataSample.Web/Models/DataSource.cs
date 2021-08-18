//-----------------------------------------------------------------------------
// <copyright file="DataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace AspNetODataSample.Web.Models
{
    public static class DataSource
    {
        private static IList<TodoItem> _items = null;
        public static IList<TodoItem> GetTodoItems()
        {
            if (_items != null)
            {
                return _items;
            }

            _items = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Name = "Walk dog",
                    IsComplete = true
                },

                new TodoItem
                {
                    Id = 2,
                    Name = "Coooking",
                    IsComplete = false
                },

                new TodoItem
                {
                    Id = 3,
                    Name = "Reading",
                    IsComplete = false
                }
            };

            return _items;
        }
    }
}
