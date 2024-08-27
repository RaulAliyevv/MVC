using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class ColorController : Controller
	{
		private readonly AppDbContext _context;

		public ColorController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Color> colors = await _context.Colors.Where(x => !x.IsDeleted).Include(c => c.ProductColors).ToListAsync();

			return View(colors);
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Color color)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			bool result = await _context.Colors.AnyAsync(x => x.Name.Trim() == color.Name.Trim());

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			color.CreatedTime = DateTime.Now;
			await _context.Colors.AddAsync(color);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Color color = await _context.Colors.FirstOrDefaultAsync(x => x.Id == id);

			if (color == null) return NotFound();

			return View(color);
		}

		[HttpPost]
		public async Task<IActionResult> Update(int? id, Color color)
		{
			if (id == null || id <= 0) return BadRequest();

			Color exists = await _context.Colors.FirstOrDefaultAsync(x => x.Id == id);

			if (color == null) return NotFound();

			bool result = await _context.Colors.AnyAsync(x => x.Name.Trim() == color.Name.Trim() && x.Id != color.Id);

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			exists.Name = color.Name;

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Color color = await _context.Colors.FirstOrDefaultAsync(x => x.Id == id);

			if (color == null) return NotFound();

			color.IsDeleted = true;
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
