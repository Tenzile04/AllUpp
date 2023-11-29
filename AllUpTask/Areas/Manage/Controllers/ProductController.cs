using AllUpTask.DAL;
using AllUpTask.Extencions;
using AllUpTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace AllUpTask.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            List<Product> products = _context.Products.ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
           
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            if (!ModelState.IsValid) return View(product);

            if (!_context.Brands.Any(x => x.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Brand Not found");
                return View();
            }
           

            bool check = false;
            if (product.TagIds != null)
            {
                foreach (var tagId in product.TagIds)
                {
                    if (!_context.Tags.Any(x => x.Id == tagId))
                    {
                        check = true;
                        break;
                    }
                }
            }
            if (check)
            {
                ModelState.AddModelError("TagId", "Tag not found");
                return View();
            }
            else
            {
                if (product.TagIds != null)
                {
                    foreach (var tagId in product.TagIds)
                    {
                        ProductTag productTag = new ProductTag
                        {
                            Product = product,
                            TagId = tagId,

                        };
                        _context.ProductTags.Add(productTag);
                    }
                }
            }
            if (product.ProductPosterImageFile != null)
            {
                if (product.ProductPosterImageFile.ContentType != "image/png" && product.ProductPosterImageFile.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("ProductPosterImageFile", "can only uploads .png or .jpeg");
                    return View();
                }

                if (product.ProductPosterImageFile.Length > 1048576)
                {
                    ModelState.AddModelError("ProductPosteImageFile", "file size must be lower than");
                    return View();
                }
                ProductImage productImage = new ProductImage
                {
                    Product = product,
                    ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", product.ProductPosterImageFile),
                    IsPoster = true
                };
                _context.ProductImages.Add(productImage);
            }
            if (product.ProductHoverImageFile != null)
            {
                if (product.ProductHoverImageFile.ContentType != "image/png" && product.ProductHoverImageFile.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("BookHoverImageFile", "can only uploads .png or .jpeg");
                    return View();
                }

                if (product.ProductHoverImageFile.Length > 1048576)
                {
                    ModelState.AddModelError("BookHoverImageFile", "file size must be lower than");
                    return View();
                }
                ProductImage productImage = new ProductImage()
                {
                    Product = product,
                    ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", product.ProductHoverImageFile),
                    IsPoster = false
                };
                _context.ProductImages.Add(productImage);
            }
            if (product.ImageFiles != null)
            {
                foreach (var imageFile in product.ImageFiles)
                {
                    if (imageFile.ContentType != "image/png" && imageFile.ContentType != "image/jpeg")
                    {
                        ModelState.AddModelError("ImageFiles", "can only uploads .png or .jpeg");
                        return View();
                    }

                    if (imageFile.Length > 1048576)
                    {
                        ModelState.AddModelError("ImageFiles", "file size must be lower than");
                        return View();
                    }
                    ProductImage bookImage = new ProductImage
                    {
                        Product = product,
                        ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", imageFile),
                        IsPoster = null
                    };
                    _context.ProductImages.Add(bookImage);
                }
            }
             _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("index");
        }
        [HttpGet]
        public IActionResult Update(int id)
        {

            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            Product existProduct = _context.Products.Include(p=>p.ProductImages).Include(x => x.ProductTags).FirstOrDefault(x => x.Id == id);
            if (existProduct == null)
            {
                return NotFound();
            };
            existProduct.TagIds = existProduct.ProductTags.Select(bt => bt.TagId).ToList();

            return View(existProduct);
        }
        [HttpPost]
        public IActionResult Update(Product product)
        {
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();

            if (!ModelState.IsValid) return View();

            Product existProduct = _context.Products.Include(p => p.ProductImages).Include(x => x.ProductTags).FirstOrDefault(b => b.Id == product.Id);
            if (existProduct == null) return NotFound();
            if (!_context.Brands.Any(g => g.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Brand not found!");
                return View();
            }

           

            var existproduct = _context.Products.Include(x => x.ProductImages).FirstOrDefault(x => x.Id == product.Id);
            if (existproduct == null)
            {
                return NotFound();
            }


            existProduct.ProductTags.RemoveAll(bt => !product.TagIds.Contains(bt.TagId));

            foreach (var tagId in product.TagIds.Where(t => !existProduct.ProductTags.Any(bt => bt.TagId == t)))
            {
                ProductTag productTag = new ProductTag
                {
                    Product=product,
                    TagId = tagId
                };
                existProduct.ProductTags.Add(productTag);
            }

            if (product.ProductPosterImageFile != null)
            {
                if (product.ProductPosterImageFile.ContentType != "image/png" && product.ProductPosterImageFile.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("ProductPosterImageFile", "can only uploads .png or .jpeg");
                    return View();
                }

                if (product.ProductPosterImageFile.Length > 1048576)
                {
                    ModelState.AddModelError("ProductPosterImageFile", "file size must be lower than");
                    return View();
                }
                existproduct.ProductImages.Remove(existProduct.ProductImages.FirstOrDefault(x=>x.IsPoster == true));

                ProductImage productImage = new ProductImage
                {
                    Product = product,
                    ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", product.ProductPosterImageFile),
                    IsPoster = true
                };
                existProduct.ProductImages.Add(productImage);
            }
            if (product.ProductHoverImageFile != null)
            {
                if (product.ProductHoverImageFile.ContentType != "image/png" && product.ProductHoverImageFile.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("BookHoverImageFile", "can only uploads .png or .jpeg");
                    return View();
                }

                if (product.ProductHoverImageFile.Length > 1048576)
                {
                    ModelState.AddModelError("BookHoverImageFile", "file size must be lower than");
                    return View();
                }

                existproduct.ProductImages.Remove(existProduct.ProductImages.FirstOrDefault(x => x.IsPoster == false));

                ProductImage productImage = new ProductImage()
                {
                    Product = product,
                    ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", product.ProductHoverImageFile),
                    IsPoster = false
                };
                existProduct.ProductImages.Add(productImage);
            }
            
            if (product.ImageFiles != null)
            {
                if (existProduct.ProductImages is not null)
                {
                    existproduct.ProductImages.RemoveAll(pi => !product.ProductImageIds.Contains(pi.Id) && pi.IsPoster != null);
                }

                foreach (var imageFile in product.ImageFiles)
                {
                    if (imageFile.ContentType != "image/png" && imageFile.ContentType != "image/jpeg")
                    {
                        ModelState.AddModelError("ImageFiles", "can only uploads .png or .jpeg");
                        return View();
                    }

                    if (imageFile.Length > 1048576)
                    {
                        ModelState.AddModelError("ImageFiles", "file size must be lower than");
                        return View();
                    }
                    ProductImage productImage = new ProductImage
                    {
                        Product = product,
                        ImageUrl = Helper.SaveFile(_env.WebRootPath, "uploads/products", imageFile),
                        IsPoster = null
                    };
                    existProduct.ProductImages.Add(productImage);
                }
            }
            existProduct.Name = product.Name;
            existProduct.Desc = product.Desc;
            existProduct.SalePrice = product.SalePrice;
            existProduct.CostPrice = product.CostPrice;
            existProduct.DiscountPercent = product.DiscountPercent;
            existProduct.IsAvailable = product.IsAvailable;
            existProduct.Tax = product.Tax;
            existProduct.Code = product.Code;
            existProduct.BrandId = product.BrandId;
            _context.SaveChanges();
            return RedirectToAction("index");
        }
        public IActionResult Delete(int id)
        {


            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            if (id == null) return NotFound();

            Product existProduct = _context.Products.FirstOrDefault(b => b.Id == id);
            if (existProduct == null) return NotFound();


            return View(existProduct);
        }

        [HttpPost]
        public IActionResult Delete(Product product)
        {
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Tags = _context.Tags.ToList();

            Product existProduct = _context.Products.FirstOrDefault(b => b.Id == product.Id);
            if (existProduct == null) return NotFound();

            _context.Products.Remove(existProduct);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}

