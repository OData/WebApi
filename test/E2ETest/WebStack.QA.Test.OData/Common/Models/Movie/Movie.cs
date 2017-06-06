using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Common.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public IEnumerable<Person> Actors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Person Director { get; set; }
        public Person Producer { get; set; }
    }

    public class Person
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public Person Partner { get; set; }
        public int Age { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
    }
}
