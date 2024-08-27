using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class CategoryController : Controller
	{
		private readonly AppDbContext _context;

		public CategoryController(AppDbContext context)
        {
			_context = context;
		}
        public async Task<IActionResult> Index()
		{
			List<Category> categories = await _context.Categories.Where(x => !x.IsDeleted).Include(c => c.Products).ToListAsync();

			return View(categories);
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Category category)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			bool result = await _context.Categories.AnyAsync(x => x.Name.Trim() == category.Name.Trim());

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			category.CreatedTime = DateTime.Now;
			await _context.Categories.AddAsync(category);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Category category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

			if (category == null) return NotFound();

			return View(category);
		}

        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
		{
            if (id == null || id <= 0) return BadRequest();

            Category exists = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null) return NotFound();

            bool result = await _context.Categories.AnyAsync(x => x.Name.Trim() == category.Name.Trim() && x.Id != category.Id);

            if (result)
            {
                ModelState.AddModelError("Name", "name exists");
                return View();
            }

            exists.Name = category.Name;

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
        }

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Category category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

			if (category == null) return NotFound();

			category.IsDeleted = true; 
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

    }
}
