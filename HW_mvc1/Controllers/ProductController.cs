using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest();
            }

			Product? product = await _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductImages.OrderByDescending(x => x.IsPrimary))
                .Include(x => x.ProductTags).ThenInclude(p => p.Tag)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (product is null)
			{
				return NotFound();
			}

			DetailVM detailVM = new DetailVM
			{
				Product = product,
				Products = await _context.Products.Where(p => p.CategoryId == product.CategoryId && p.Id != id)
					.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
					.ToListAsync(),
			};

			return View(detailVM);
		}

        public IActionResult Index()
        {
            return View();
        }
    }
}
