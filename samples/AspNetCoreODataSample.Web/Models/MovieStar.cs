namespace AspNetCoreODataSample.Web.Models
{
    public class MovieStar
    {
        public Movie Movie { get; set; }

        public int MovieId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}