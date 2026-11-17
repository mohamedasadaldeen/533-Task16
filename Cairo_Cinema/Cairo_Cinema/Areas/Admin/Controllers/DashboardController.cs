using Cairo_Cinema.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cairo_Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN},{CD.ADMIN},{CD.EMPLOYEE}")]   
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new DashboardVM
            {
                MoviesCount = _context.Movies.Count(),
                CategoriesCount = _context.Categories.Count(),
                CinemasCount = _context.Cinemas.Count(),
                ActorsCount = _context.Actors.Count(),
                UsersCount = _context.Users.Count(),

                MoviesPerCategory = _context.Movies
                    .GroupBy(m => m.Category.Name)
                    .Select(g => new ChartItem { Label = g.Key, Value = g.Count() })
                    .ToList(),

                MoviesPerCinema = _context.Movies
                    .GroupBy(m => m.Cinema.Name)
                    .Select(g => new ChartItem { Label = g.Key, Value = g.Count() })
                    .ToList()
            };

            return View(vm);
        }
    }
}
