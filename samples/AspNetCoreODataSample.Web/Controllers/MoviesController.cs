// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreODataSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.Web.Controllers
{
    public class MoviesController : ODataController, IDisposable
    {
        private readonly MovieContext _context;

        private readonly IList<Movie> _inMemoryMovies;

        public MoviesController(MovieContext context)
        {
            _context = context;

            if (_context.Movies.Count() == 0)
            {
                Movie conanMovie = new Movie
                {
                    Title = "Conan",
                    ReleaseDate = new DateTimeOffset(new DateTime(2017, 3, 3)),
                    Genre = Genre.Comedy,
                    Price = 1.99m
                };
                Movie dieHardMovie = new Movie
                {
                    Title = "Die Hard",
                    ReleaseDate = new DateTimeOffset(new DateTime(2014, 1, 3)),
                    Genre = Genre.Comedy,
                    Price = 1.89m
                };
                _context.Movies.Add(conanMovie);
                _context.Movies.Add(dieHardMovie);
                _context.SaveChanges();
                MovieStar s = new MovieStar
                {
                    FirstName = "Arnold",
                    LastName = "Schwarzenegger",
                    MovieId = conanMovie.ID
                };
                _context.MovieStars.Add(s);
                MovieStar b = new MovieStar
                {
                    FirstName = "Bruce",
                    LastName = "Willis",
                    MovieId = dieHardMovie.ID
                };
                _context.MovieStars.Add(b);
                _context.SaveChanges();
            }

            _inMemoryMovies = new List<Movie>
            {
                new Movie
                {
                    ID = 1,
                    Title = "Conan",
                    ReleaseDate = new DateTimeOffset(new DateTime(2018, 3, 3)),
                    Genre = Genre.Comedy,
                    Price = 1.99m,
                    Stars = new List<MovieStar>{
                        new MovieStar
                        {
                            FirstName = "Arnold",
                            LastName = "Schwarzenegger",
                            MovieId = 1
                        },
                        new MovieStar
                        {
                            FirstName = "Jackie",
                            LastName = "Chan",
                            MovieId = 1
                        }
                    }
                },
                new Movie
                {
                    ID = 2,
                    Title = "James",
                    ReleaseDate = new DateTimeOffset(new DateTime(2017, 3, 3)),
                    Genre = Genre.Adult,
                    Price = 91.99m,
                    Stars = new List<MovieStar>{
                        new MovieStar
                        {
                            FirstName = "Bruce",
                            LastName = "Willis",
                            MovieId = 2
                        }
                    }
                }
            };
        }

        [EnableQuery]
        public IActionResult Get()
        {
            if (Request.Path.Value.Contains("efcore"))
            {
                return Ok(_context.Movies);
            }
            else
            {
                return Ok(_inMemoryMovies);
            }
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            Movie m;
            if (Request.Path.Value.Contains("efcore"))
            {
                m = _context.Movies.FirstOrDefault(c => c.ID == key);
            }
            else
            {
                m = _inMemoryMovies.FirstOrDefault(c => c.ID == key);
            }

            if (m == null)
            {
                return NotFound();
            }

            return Ok(m);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
