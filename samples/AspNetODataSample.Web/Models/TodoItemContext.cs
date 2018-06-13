// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Data.Entity;

namespace AspNetODataSample.Web.Models
{
    public class TodoItemContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}