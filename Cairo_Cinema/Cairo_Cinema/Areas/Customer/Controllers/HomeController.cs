using Cairo_Cinema.Data;
using Cairo_Cinema.Models;
using Cairo_Cinema.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Cairo_Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;// = new ApplicationDbContext();
        private readonly IRepository<Movie> _movieRepository;// = new ApplicationDbContext();

        public HomeController(ApplicationDbContext context, IRepository<Movie> movieRepository)
        {
            _context = context;
            _movieRepository = movieRepository;
        }

        public async Task<IActionResult> Index(FilterVM filter)
        {
            //var movies = _context.Movies.AsQueryable();
            //movies = movies.Include(m => m.Category).Include(m => m.Actor);
            var movies = await _movieRepository.GetListAsync(includes : [m => m.Category, m => m.Actor]);

            if (filter._Price > 0)
            {
                movies = movies.Where(m => m.Price.Equals(filter._Price));
            }

            if (filter.RateId > 0)
            {
                movies = movies.Where(m => m.Rate >= filter.RateId);
            }

            if (filter.GenreId > 0)
            {
                movies = movies.Where(m => m.CategoryId == filter.GenreId);
            }

            if (filter.CinemaId > 0)
            {
                movies = movies.Where(m => m.CinemaId == filter.CinemaId);
            }

            ViewBag.Cinemas = _context.Cinemas;
            ViewBag.Categories = _context.Categories;
            //Pagination
            ViewBag.TotalPages = (int)Math.Ceiling(movies.Count() / 4.0);
            ViewBag.CurrentPage = filter.Page;
            movies = movies.Skip((filter.Page - 1) * 4).Take(4);

            return View(movies.ToList());
        }

        public async Task<IActionResult> CinemaDetails(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m=> m.Id == id);
            if (movie == null) return NotFound();
            var relatedMovies = await _movieRepository.GetListAsync(includes: [m => m.Category, m => m.Actor, m => m.Cinema]);
            relatedMovies = relatedMovies.Where(m => m.Id != id).OrderBy(m => Guid.NewGuid()).Take(4);
            return View(new MovieWithRelatedVM(){
                Movie = movie,
                RelatedMovies = relatedMovies,
            });
        }

    }
}
