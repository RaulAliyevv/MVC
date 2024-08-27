using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class TagController : Controller
	{
		private readonly AppDbContext _context;

		public TagController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			List<Tag> tags = await _context.Tags.Where(x => !x.IsDeleted).Include(c => c.ProductTags).ToListAsync();

			return View(tags);
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Tag tag)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			bool result = await _context.Tags.AnyAsync(x => x.Name.Trim() == tag.Name.Trim());

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			tag.CreatedTime = DateTime.Now;
			await _context.Tags.AddAsync(tag);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Tag tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);

			if (tag == null) return NotFound();

			return View(tag);
		}

		[HttpPost]
		public async Task<IActionResult> Update(int? id, Tag tag)
		{
			if (id == null || id <= 0) return BadRequest();

			Tag exists = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);

			if (tag == null) return NotFound();

			bool result = await _context.Tags.AnyAsync(x => x.Name.Trim() == tag.Name.Trim() && x.Id != tag.Id);

			if (result)
			{
				ModelState.AddModelError("Name", "name exists");
				return View();
			}

			exists.Name = tag.Name;

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Tag tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);

			if (tag == null) return NotFound();

			tag.IsDeleted = true;
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
