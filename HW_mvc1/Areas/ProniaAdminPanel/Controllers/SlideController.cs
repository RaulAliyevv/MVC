using HW_mvc1.Areas.ProniaAdminPanel.ViewModels;
using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.Utilities.Enums;
using HW_mvc1.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class SlideController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
			_context = context;
			_env = env;
		}
        public async Task<IActionResult> Index()
		{
			List<Slide> slides = await _context.Slides.Where(x => !x.IsDeleted).ToListAsync();
			return View(slides);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateSlideVM slideVM)
		{
			//if(!ModelState.IsValid) return View();

			if (!slideVM.Photo.ValidateType("image/"))
			{
				ModelState.AddModelError("Photo", "File type isnt correct");
				return View();
			}

			if(!slideVM.Photo.ValidateSize(FileSize.MB, 2))
			{
				ModelState.AddModelError("Photo", "File size must be <= 2mb");
				return View();
			}


			string filename = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

			Slide slide = new Slide
			{
				Title = slideVM.Title,
				Subtitle = slideVM.Subtitle,
				Description = slideVM.Description,
				Order = slideVM.Order,
				ImageUrl = filename
			};

			slide.CreatedTime = DateTime.Now;
			await _context.Slides.AddAsync(slide);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Slide slide = await _context.Slides.FirstOrDefaultAsync(x => x.Id == id);

			if (slide is null) return NotFound();

			slide.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");

			_context.Remove(slide);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id <= 0) return BadRequest();

			Slide slide = await _context.Slides.FirstOrDefaultAsync(x => x.Id == id);

			if (slide is null) return NotFound();

			UpdateSlideVM slideVM = new UpdateSlideVM
			{
				Title = slide.Title,
				Subtitle = slide.Subtitle,
				Description = slide.Description,
				Order = slide.Order,
				ImageUrl = slide.ImageUrl
			};

			return View(slideVM);
		}

		[HttpPost]
		public async Task<IActionResult> Update(int? id, UpdateSlideVM slideVM)
		{
			if(!ModelState.IsValid) return View(slideVM);

			Slide existed = await _context.Slides.FirstOrDefaultAsync(x => x.Id == id);
			slideVM.ImageUrl = existed.ImageUrl;
			if(existed is null) return NotFound();

			if(slideVM.Photo is not null)
			{
				if (!slideVM.Photo.ValidateType("image/"))
				{
					ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type isnt correct");
					return View(slideVM);
				}
				if (!slideVM.Photo.ValidateSize(FileSize.MB, 2))
				{
					ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File size must be <=2");
					return View(slideVM);
				}
				string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
				existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
				existed.ImageUrl = fileName;
			}

			existed.Title = slideVM.Title;
			existed.Description = slideVM.Description;
			existed.Subtitle = slideVM.Subtitle;
			existed.Order = slideVM.Order;

			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

	}
}
