using System;
using System.Collections.Generic;
using System.Linq;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common.Models.Products;

namespace WebStack.QA.Test.OData.Common.Models
{
    public static class ModelHelper
    {
        public static IEnumerable<Product> CreateRandomProducts()
        {
            int seed = RandomSeedGenerator.GetRandomSeed();
            var r = new Random(seed);
            var products = new List<Product>();

            for (int i = 0; i < r.Next(5000); i++)
            {
                products.Add(new Product
                {
                    ID = r.Next(1000),
                    Name = InstanceCreator.CreateInstanceOf<string>(r),
                    Price = InstanceCreator.CreateInstanceOf<Decimal>(r),
                    Amount = InstanceCreator.CreateInstanceOf<double>(r),
                    Rating = r.Next(5),
                    ReleaseDate = InstanceCreator.CreateInstanceOf<DateTime?>(r),
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
            Person mike = new Person { PersonId = 1, Name = "Mike", Tags = new string[] { "Favorit" }, Age = 45 };
            Person tom = new Person { PersonId = 2, Name = "Tom", Tags = null, Age = 28 };
            Person kevin = new Person { PersonId = 3, Name = "Kevin", Tags = new string[] { "Favorit", "Super Star" }, Age = 30 };
            Person rose = new Person { PersonId = 4, Name = "Rose", Tags = null, Spouse = kevin, Age = 22 };
            kevin.Spouse = rose;
            return new Movie[] 
            {
                new Movie 
                {
                    MovieId = 1,
                    Actors = new Person[] 
                    {
                        mike,
                        tom
                    },
                    Director = kevin,
                    Tags = new string[] { "Quirky" }
                },
                new Movie 
                {
                    MovieId = 2,
                    Actors = new Person[] 
                    {
                        kevin
                    },
                    Director = kevin,
                    Tags = new string[] { "Fiction" }
                },
                new Movie 
                {
                    MovieId = 3,
                    Actors = new Person[] 
                    {
                        mike,
                        tom,
                        kevin
                    },
                    Director = kevin,
                    Producer = kevin,
                    Tags = new string[] { "Animation" }
                }
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
