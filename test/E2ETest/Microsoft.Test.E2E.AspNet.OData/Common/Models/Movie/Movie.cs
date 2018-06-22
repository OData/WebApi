// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public IEnumerable<Person> Actors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Person Director { get; set; }
        public Person Producer { get; set; }
        public IEnumerable<Theater> Showings { get; set; }
        public IEnumerable<int> Sales { get; set; }
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
}
