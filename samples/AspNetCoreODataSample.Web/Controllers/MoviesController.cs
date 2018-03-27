// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using AspNetCoreODataSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.Web.Controllers
{
    public class MoviesController : ODataController
    {
        private readonly MovieContext _context;

        public MoviesController(MovieContext context)
        {
            _context = context;

            if (_context.Movies.Count() == 0)
            {
                Movie m = new Movie
                {
                    Title = "Conan",
                    ReleaseDate = new DateTimeOffset(new DateTime(2017, 3, 3)),
                    Genre = Genre.Comedy,
                    Price = 1.99m
                };
                _context.Movies.Add(m);
                _context.SaveChanges();
            }
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Movies);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            Movie m = _context.Movies.FirstOrDefault(c => c.ID == key);
            if (m == null)
            {
                return NotFound();
            }

            return Ok(m);
        }
    }
}
