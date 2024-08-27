using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class SizeController : Controller
	{
		private readonly AppDbContext _context;

		public SizeController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Size> sizes = await _context.Sizes.Where(x => !x.IsDeleted).Include(c => c.ProductSizes).ToListAsync();

			return View(sizes);
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Size size)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			bool result = await _context.Sizes.AnyAsync(x => x.Name.Trim() == size.Name.Trim());

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			size.CreatedTime = DateTime.Now;
			await _context.Sizes.AddAsync(size);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Size size = await _context.Sizes.FirstOrDefaultAsync(x => x.Id == id);

			if (size == null) return NotFound();

			return View(size);
		}

		[HttpPost]
		public async Task<IActionResult> Update(int? id, Size size)
		{
			if (id == null || id <= 0) return BadRequest();

			Size exists = await _context.Sizes.FirstOrDefaultAsync(x => x.Id == id);

			if (size == null) return NotFound();

			bool result = await _context.Sizes.AnyAsync(x => x.Name.Trim() == size.Name.Trim() && x.Id != size.Id);

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			exists.Name = size.Name;

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Size size = await _context.Sizes.FirstOrDefaultAsync(x => x.Id == id);

			if (size == null) return NotFound();

			size.IsDeleted = true;
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
