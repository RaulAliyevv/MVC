using System.Collections.Immutable;
using HW_mvc1.Areas.ProniaAdminPanel.ViewModels;
using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.Utilities.Enums;
using HW_mvc1.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
    [Area("ProniaAdminPanel")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<GetAdminProductVM> products = await _context.Products
                .Where(x => !x.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .Select(p => new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages.FirstOrDefault().ImageUrl
                })
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync(),
                Colors = await _context.Colors.Where(x => x.IsDeleted == false).ToListAsync(),
                Sizes = await _context.Sizes.Where(x => x.IsDeleted == false).ToListAsync(),
                Tags = await _context.Tags.Where(x => x.IsDeleted == false).ToListAsync()
            };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync();
            productVM.Colors = await _context.Colors.Where(x => x.IsDeleted == false).ToListAsync();
            productVM.Sizes = await _context.Sizes.Where(x => x.IsDeleted == false).ToListAsync();
            productVM.Tags = await _context.Tags.Where(x => x.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid)
            {

                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("MainPhoto", "image type incorrect");

                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("MainPhoto", "image size incorrect( <= 2mb)");

                return View(productVM);
            }

            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "image type incorrect");

                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("HoverPhoto", "image size incorrect( <= 2mb)");

                return View(productVM);
            }

            bool result = await _context.Categories.AnyAsync(x => x.Id == productVM.CategoryId && x.IsDeleted == false);
            if (!result)
            {
                ModelState.AddModelError("CategoryId", "Category doesnt exist");

                return View(productVM);
            }

            if (productVM.TagIds is not null)
            {
                bool tagresult = productVM.TagIds.Any(tid => !productVM.Tags.Exists(t => t.Id == tid));
                if (tagresult)
                {
                    ModelState.AddModelError("TagIds", "tag doesnt exist");
                    return View(productVM);
                }
            }

            if (productVM.ColorIds is not null)
            {
                bool colorresult = productVM.ColorIds.Any(tid => !productVM.Colors.Exists(t => t.Id == tid));
                if (colorresult)
                {
                    ModelState.AddModelError("ColorIds", "color doesnt exist");
                    return View(productVM);
                }
            }

            if (productVM.SizeIds is not null)
            {
                bool sizeresult = productVM.SizeIds.Any(tid => !productVM.Sizes.Exists(t => t.Id == tid));
                if (sizeresult)
                {
                    ModelState.AddModelError("SizeIds", "size doesnt exist");
                    return View(productVM);
                }
            }

            ProductImage mainimg = new ProductImage
            {
                CreatedTime = DateTime.Now,
                IsPrimary = true,
                ImageUrl = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

            ProductImage hoverimg = new ProductImage
            {
                CreatedTime = DateTime.Now,
                IsPrimary = false,
                ImageUrl = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

            Product product = new Product
            {
                CategoryId = productVM.CategoryId.Value,
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CreatedTime = DateTime.Now,
                ProductImages = new List<ProductImage> { mainimg, hoverimg }
            };

            if (productVM.Photos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.Photos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"{file.Name} file type incorrect";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"{file.Name} file size incorrect";
                        continue;
                    }
                    ProductImage img = new ProductImage
                    {
                        CreatedTime = DateTime.Now,
                        IsPrimary = null,
                        ImageUrl = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                    };
                    product.ProductImages.Add(img);
                }
                TempData["ErrorMessage"] = text;
            }

            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tid => new ProductTag
                {
                    TagId = tid

                }).ToList();
            }

            if (productVM.ColorIds is not null)
            {
                product.ProductColors = productVM.ColorIds.Select(cid => new ProductColor
                {
                    ColorId = cid

                }).ToList();
            }

            if (productVM.SizeIds is not null)
            {
                product.ProductSizes = productVM.SizeIds.Select(sid => new ProductSize
                {
                    SizeId = sid

                }).ToList();
            }

            //instead of select you can write:
            //foreach (var tagid in productVM.TagIds)
            //{
            //    ProductTag prodtag = new ProductTag
            //    {
            //        TagId = tagid,
            //        Product = product
            //    };
            //    _context.ProductTags.Add(prodtag);
            //}

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            Product product = await _context.Products.Include(x => x.ProductImages).Include(x => x.ProductTags).Include(x => x.ProductSizes).Include(x => x.ProductColors).FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (product == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync(),
                Colors = await _context.Colors.Where(x => !x.IsDeleted).ToListAsync(),
                Sizes = await _context.Sizes.Where(x => !x.IsDeleted).ToListAsync(),
                Tags = await _context.Tags.Where(x => !x.IsDeleted).ToListAsync(),
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ColorIds = product.ProductColors.Select(pc => pc.ColorId).ToList(),
                SizeIds = product.ProductSizes.Select(ps => ps.SizeId).ToList(),
                Images = product.ProductImages.ToList()
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id <= 0) return BadRequest();

            Product existed = await _context.Products.Include(x => x.ProductImages).Include(x => x.ProductTags).Include(x => x.ProductSizes).Include(x => x.ProductColors).FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (existed == null) return NotFound();

            productVM.Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Tags = await _context.Tags.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Colors = await _context.Colors.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Sizes = await _context.Sizes.Where(x => !x.IsDeleted).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("MainPhoto", "image type incorrect");

                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError("MainPhoto", "image size incorrect( <= 2mb)");

                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("HoverPhoto", "image type incorrect");

                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError("HoverPhoto", "image size incorrect( <= 2mb)");

                    return View(productVM);
                }
            }

            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = await _context.Categories.AnyAsync(x => x.Id == productVM.CategoryId && x.IsDeleted == false);
                if (!result)
                {
                    ModelState.AddModelError("CategoryId", "category does not exist");
                    return View(productVM);
                }
            }

            if (productVM.MainPhoto is not null)
            {
                ProductImage mainimg = new ProductImage
                {
                    CreatedTime = DateTime.Now,
                    IsPrimary = true,
                    ImageUrl = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                };

                ProductImage mainexisted = existed.ProductImages.FirstOrDefault(x => x.IsPrimary == true);
                mainexisted.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(mainexisted);
                existed.ProductImages.Add(mainimg);
            }

            if (productVM.HoverPhoto is not null)
            {
                ProductImage hoverimg = new ProductImage
                {
                    CreatedTime = DateTime.Now,
                    IsPrimary = true,
                    ImageUrl = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                };

                ProductImage hoverexisted = existed.ProductImages.FirstOrDefault(x => x.IsPrimary == false);
                hoverexisted.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(hoverexisted);
                existed.ProductImages.Add(hoverimg);
            }

            var deleteimages = existed.ProductImages.Where(p => !productVM.ImageIds.Exists(imgid => p.Id == imgid) && p.IsPrimary == null).ToList();
            foreach (var rmvimg in deleteimages)
            {
                rmvimg.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                //existed.ProductImages.Remove(rmvimg);
            }

            if (productVM.Photos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in productVM.Photos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"{file.Name} file type incorrect";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"{file.Name} file size incorrect";
                        continue;
                    }
                    ProductImage img = new ProductImage
                    {
                        CreatedTime = DateTime.Now,
                        IsPrimary = null,
                        ImageUrl = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                    };
                    existed.ProductImages.Add(img);
                }
                TempData["ErrorMessage"] = text;
            }


            _context.ProductTags.RemoveRange(existed.ProductTags.Where(pt => !productVM.TagIds.Exists(tid => tid == pt.Id)).ToList());
            _context.ProductColors.RemoveRange(existed.ProductColors.Where(pc => !productVM.ColorIds.Exists(cid => cid == pc.Id)).ToList());
            _context.ProductSizes.RemoveRange(existed.ProductSizes.Where(ps => !productVM.SizeIds.Exists(sid => sid == ps.Id)).ToList());

            existed.ProductTags.AddRange(productVM.TagIds.Where(tid => existed.ProductTags.Any(pt => pt.TagId == tid)).Select(tId => new ProductTag { TagId = tId }));
            existed.ProductColors.AddRange(productVM.ColorIds.Where(cid => existed.ProductColors.Any(pc => pc.ColorId == cid)).Select(cId => new ProductColor { ColorId = cId }));
            existed.ProductSizes.AddRange(productVM.SizeIds.Where(tid => existed.ProductSizes.Any(pt => pt.SizeId == tid)).Select(tId => new ProductSize { SizeId = tId }));


            existed.Name = productVM.Name;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price;
            existed.SKU = productVM.SKU;


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            Product product = await _context.Products.Include(x => x.ProductTags)
                                    .Include(x => x.ProductSizes)
                                    .Include(x => x.ProductColors)
                                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (product == null) return NotFound();

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
