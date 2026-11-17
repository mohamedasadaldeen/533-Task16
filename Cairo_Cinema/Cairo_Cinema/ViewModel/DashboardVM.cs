namespace Cairo_Cinema.ViewModel
{
    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class DashboardVM
    {
        public int MoviesCount { get; set; }
        public int CategoriesCount { get; set; }
        public int CinemasCount { get; set; }
        public int ActorsCount { get; set; }
        public int UsersCount { get; set; }

        public List<ChartItem> MoviesPerCategory { get; set; } = new();
        public List<ChartItem> MoviesPerCinema { get; set; } = new();
    }
}
