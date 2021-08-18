//-----------------------------------------------------------------------------
// <copyright file="Movie.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public Genre MainGenre { get; set; }
        public IEnumerable<Person> Actors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Person Director { get; set; }
        public Person Producer { get; set; }
        public IEnumerable<Theater> Showings { get; set; }
        public IEnumerable<int> Sales { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
    }

    public class Person
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Person Partner { get; set; }
        public Person Parent { get; set; }
        public int Age { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public IEnumerable<Theater> FavoriteTheaters { get; set; }
        public Theater LastVisited { get; set; }
    }

    public class Teenager : Person
    {
        public int TeenageId { get; set; }
    }

    public class Theater
    {
        public int TheaterId { get; set; }
        public string Name { get; set; }
    }

    public enum Genre
    {
        Adventure,
        Horror,
        Comedy,
        Drama,
    }
}
