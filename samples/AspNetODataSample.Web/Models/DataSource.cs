// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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