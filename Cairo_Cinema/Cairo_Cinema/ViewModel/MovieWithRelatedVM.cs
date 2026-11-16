namespace Cairo_Cinema.ViewModel
{
    public class MovieWithRelatedVM
    {
        public Movie Movie { get; set; }
        public IEnumerable<Movie> RelatedMovies { get; set; }

    }
}
