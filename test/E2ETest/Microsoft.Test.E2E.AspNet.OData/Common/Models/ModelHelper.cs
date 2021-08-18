//-----------------------------------------------------------------------------
// <copyright file="ModelHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Products;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models
{
    public static class ModelHelper
    {
        public static IEnumerable<Product> CreateRandomProducts()
        {
            int seed = RandomSeedGenerator.GetRandomSeed();
            var r = new Random(seed);
            var products = new List<Product>();
            try
            {
                for (int i = 0; i < r.Next(5000); i++)
                {
                    products.Add(new Product
                    {
                        ID = r.Next(1000),
                        Name = InstanceCreator.CreateInstanceOf<string>(r),
                        Price = InstanceCreator.CreateInstanceOf<Decimal>(r),
                        Rating = r.Next(5),
                        PublishDate = InstanceCreator.CreateInstanceOf<DateTimeOffset>(r),
                        ReleaseDate = InstanceCreator.CreateInstanceOf<DateTimeOffset?>(r),

                        Date = InstanceCreator.CreateInstanceOf<DateTime>(r),
                        NullableDate = r.Next(9)%3 == 0 ? (Date?) null : InstanceCreator.CreateInstanceOf<DateTime>(r),

                        TimeOfDay = InstanceCreator.CreateInstanceOf<DateTimeOffset>(r).TimeOfDay,
                        NullableTimeOfDay = r.Next(19) % 3 == 0 
                            ? (TimeOfDay?)null 
                            : InstanceCreator.CreateInstanceOf<DateTimeOffset>(r).TimeOfDay,
                        Taxable = InstanceCreator.CreateInstanceOf<bool?>(r)
                    });

                    if (r.NextDouble() > .7)
                    {
                        products.Last().Supplier = new Supplier
                        {
                            ID = r.Next(1000),
                            Name = InstanceCreator.CreateInstanceOf<string>(r),
                            Address = new Address
                            {
                                City = InstanceCreator.CreateInstanceOf<string>(r),
                                State = InstanceCreator.CreateInstanceOf<string>(r)
                            }
                        };

                        products.Last().Supplier.Products.Add(products.Last());
                    }
                    else if (r.NextDouble() > .3)
                    {
                        products.Last().Supplier = new Supplier
                        {
                            ID = r.Next(1000),
                            Name = InstanceCreator.CreateInstanceOf<string>(r),
                            Address = null
                        };

                        products.Last().Supplier.Products.Add(products.Last());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return products;
        }

        public static IEnumerable<Order> CreateOrderData()
        {
            Product kinect = new Product { ID = 0, Name = "Kinect" };
            Product xbox = new Product { ID = 1, Name = "XBox" };
            return new Order[] 
            {
                new Order 
                {
                    OrderId = 1,
                    OrderLines = new OrderLine[] 
                    {
                        new OrderLine 
                        {
                            OrderLineId = 1,
                            Cost = 100M,
                            Product = kinect
                        },
                        new OrderLine 
                        {
                            OrderLineId = 2,
                            Cost = 300M,
                            Product = xbox
                        }
                    }
                },
                new Order 
                {
                    OrderId = 2,
                    OrderLines = new OrderLine[] 
                    {
                        new OrderLine 
                        {
                            OrderLineId = 3,
                            Cost = 300M,
                            Product = xbox
                        }
                    }
                }
            };
        }

        public static IEnumerable<Movie> CreateMovieData()
        {
            Theater theater1 = new Theater { TheaterId = 1, Name = "Happy" };
            Theater theater2 = new Theater { TheaterId = 2, Name = "Sad" };
            Theater theater3 = new Theater { TheaterId = 3, Name = "Funny" };
            Theater theater4 = new Theater { TheaterId = 4, Name = "Angry" };
            Theater[] favoriteTheaters = new Theater[] { theater1, theater2, theater3 };
            Theater[] showingTheaters = new Theater[] { theater1, theater2, theater3 };
            Person mike = new Person
            {
                PersonId = 1,
                Name = "Mike",
                Tags = new string[] { "Favorite" },
                Age = 45,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater1
            };
            Person tom = new Person
            {
                PersonId = 2,
                Name = "Tom",
                Tags = null,
                Age = 28,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater2
            };
            Person kevin = new Person
            {
                PersonId = 3,
                Name = "Kevin",
                Tags = new string[] { "Favorite", "Super Star" },
                Age = 30,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater3
            };
            Person rose = new Person
            {
                PersonId = 4,
                Name = "Rose",
                Tags = null,
                Partner = kevin,
                Age = 22,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater4
            };
            Teenager jill = new Teenager
            {
                PersonId = 5,
                TeenageId = 1,
                Name = "Jill",
                Tags = null,
                Age = 12,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater4
            };
            Teenager james = new Teenager
            {
                PersonId = 6,
                TeenageId = 2,
                Name = "James",
                Tags = null,
                Age = 15,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater4
            };
            Teenager mary = new Teenager
            {
                PersonId = 6,
                TeenageId = 3,
                Name = "Mary",
                Tags = null,
                Age = 13,
                FavoriteTheaters = favoriteTheaters,
                LastVisited = theater4
            };
            kevin.Partner = rose;
            mike.Parent = tom;
            tom.Parent = kevin;
            kevin.Parent = rose;
            rose.Parent = tom;
            return new Movie[]
            {
                new Movie
                {
                    MovieId = 1,
                    Title = "movie1",
                    Summary = "summary1",
                    MainGenre = Genre.Adventure,
                    Actors = new Person[]
                    {
                        mike,
                        tom
                    },
                    Director = kevin,
                    Producer = tom,
                    Tags = new string[] { "Quirky" },
                    Showings = showingTheaters,
                    Sales = new int[] { 100, 200, 300 },
                    Genres = new Genre[] { Genre.Adventure, Genre.Comedy, Genre.Drama, Genre.Horror },
                },
                new Movie 
                {
                    MovieId = 2,
                    Title = "movie2",
                    Summary = "summary2",
                    MainGenre = Genre.Adventure,
                    Actors = new Person[] 
                    {
                        kevin
                    },
                    Director = kevin,
                    Producer = jill,
                    Tags = new string[] { "Fiction" },
                    Showings = showingTheaters,
                    Sales = new int[] { 200, 300, 400 },
                    Genres = new Genre[] { Genre.Adventure, Genre.Comedy },
                },
                new Movie 
                {
                    MovieId = 3,
                    Title = "movie3",
                    Summary = "summary3",
                    MainGenre = Genre.Drama,
                    Actors = new Person[] 
                    {
                        mike,
                        tom,
                        kevin
                    },
                    Director = kevin,
                    Producer = kevin,
                    Tags = new string[] { "Animation" },
                    Showings = showingTheaters,
                    Sales = new int[] { 1, 2, 3 },
                    Genres = new Genre[] { Genre.Drama, Genre.Horror },
                },
                new Movie
                {
                    MovieId = 4,
                    Title = "movie4",
                    Summary = "summary4",
                    MainGenre = Genre.Comedy,
                    Actors = new Teenager[]
                    {
                        jill,
                        james
                    },
                    Director = kevin,
                    Producer = james,
                    Tags = new string[] { "Fun" },
                    Showings = showingTheaters,
                    Sales = new int[] { 1, 2, 3 },
                    Genres = new Genre[] { Genre.Adventure, Genre.Drama },
                },
                new Movie
                {
                    MovieId = 5,
                    Title = "movie5",
                    Summary = "summary5",
                    MainGenre = Genre.Adventure,
                    Actors = new Person[]
                    {
                        jill,
                        james,
                        kevin
                    },
                    Director = kevin,
                    Producer = james,
                    Tags = new string[] { "Boring" },
                    Showings = showingTheaters,
                    Sales = new int[] { 1, 2, 3 },
                    Genres = new Genre[] { Genre.Comedy, Genre.Horror },
                },
            };
        }

        public static IEnumerable<Movie> CreateMovieBigData()
        {
            var persons = new List<Person>();
            for (int i = 0; i < 100; i++)
            {
                persons.Add(new Person
                {
                    Name = "Test" + i,
                    PersonId = i
                });
            }

            var movies = new List<Movie>();
            for (int i = 0; i < 100; i++)
            {
                movies.Add(new Movie()
                {
                    MovieId = i,
                });
            }

            foreach (var p in persons)
            {
                p.Movies = movies;
            }

            foreach (var m in movies)
            {
                m.Actors = persons;
            }
            return movies;
        }
    }
}
