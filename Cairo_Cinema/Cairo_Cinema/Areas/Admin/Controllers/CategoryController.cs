using Cairo_Cinema.Data;
using Cairo_Cinema.Models;
using Cairo_Cinema.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cairo_Cinema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN},{CD.ADMIN},{CD.EMPLOYEE}")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;// = new Repository<Category>();

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        //Index
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetListAsync();

            return View(categories.AsEnumerable());
        }

        //Get Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //Post Create
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get Update
        [HttpGet]
        [Authorize(Roles = $"{CD.SUPER_ADMIN},{CD.ADMIN}")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            return View(category);
        }

        //Post Update
        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN},{CD.ADMIN}")]
        public async Task<IActionResult> Update(Category category)
        {

            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{CD.SUPER_ADMIN},{CD.ADMIN}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            _categoryRepository.Remove(category);
            await _categoryRepository.CommitAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
