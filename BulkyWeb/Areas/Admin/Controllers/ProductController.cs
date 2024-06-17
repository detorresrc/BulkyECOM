using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private IProductRepository Product => _unitOfWork.Product;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> categories = Product.GetAll(includeProperties:"Category").ToList();
            
            return View(categories);
        }

        public IActionResult Upsert(int? productId)
        {
            ProductVM productVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = this.GetCategoryList()
            };

            if (!(productId == null || productId == 0))
            {
                productVM.Product = Product.Get(c => c.Id == productId, includeProperties: "ProductImages");
            }

            return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    try
                    {
                        string message;
                        if(productVM.Product.Id == 0)
                        {
                            Product.Add(productVM.Product);
                            message = "Product created successfully";
                        }
                        else
                        {
                            Product.Update(productVM.Product);
                            message = "Product updated successfully";
                        }
                        _unitOfWork.Save();
                
                        if (files != null)
                        {
                            string webRootPath = _hostingEnvironment.WebRootPath;
                    
                            foreach (IFormFile file in files)
                            {
                                string filename = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                var productPath = Path.Combine(webRootPath, "images", "products", productVM.Product.Id.ToString());

                                if (!Directory.Exists(productPath))
                                {
                                    Directory.CreateDirectory(productPath);
                                }

                                var productImagePath = Path.Combine(productPath, filename);
                                using (var fileString = new FileStream(productImagePath, FileMode.Create))
                                {
                                    file.CopyTo(fileString);
                                }
                        
                                productVM.Product.ProductImages.Add(new ProductImage
                                {
                                    ImageUrl = Path.DirectorySeparatorChar + Path.Combine("images", "products", productVM.Product.Id.ToString(), filename)
                                });
                            }
                        }
                        _unitOfWork.Save();
                        transaction.Commit();
                
                        TempData["success"] = message;
                        return RedirectToAction("Index");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        TempData["error"] = "Error while saving";
                        return RedirectToAction("Index");
                    }
                }
            }

            productVM.CategoryList = this.GetCategoryList();

            return View(productVM);
        }

        #region API CALL
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Product product = Product.Get(c => c.Id == id);
            if (product == null)
            {
                return Json(new { success = true, message = "Product not found!" });
            }

            string webRootPath = _hostingEnvironment.WebRootPath;
            var productPath = Path.Combine(webRootPath, "images", "products", id.ToString());

            if (Directory.Exists(productPath))
            {
                Directory.Delete(productPath, true);
            }
            
            Product.Remove(product);
            Product.Save();

            return Json(new { success = true, message = "Product deleted successfully"});
        }

        public IActionResult DeleteImage(int id)
        {
            ProductImage productImage = _unitOfWork.ProductImage.Get(c => c.Id == id);
            if (productImage == null)
            {
                TempData["error"] = "Product Image not found!";
                return RedirectToAction(nameof(Index));
            }

            int productId = productImage.ProductId;

            string webRootPath = _hostingEnvironment.WebRootPath;
            string imagePath = Path.Combine(webRootPath, productImage.ImageUrl.TrimStart('\\').TrimStart('/'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _unitOfWork.ProductImage.Remove(productImage);
            _unitOfWork.Save();

            TempData["success"] = "Image deleted successfully";
            return RedirectToAction(nameof(Upsert), new { productId = productId });
        }
        #endregion

        #region Private Methods
        private IEnumerable<SelectListItem> GetCategoryList()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            return CategoryList;
        }

        private void DeleteOldImage(Product product)
        {
            // string oldFile = product.ImageUrl;
            //
            // if (!string.IsNullOrEmpty(oldFile))
            // {
            //     // Delete old file
            //     var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, oldFile.TrimStart('\\').TrimStart('/'));
            //     if (System.IO.File.Exists(oldFilePath))
            //     {
            //         System.IO.File.Delete(oldFilePath);
            //     }
            // }
        }
        #endregion
    }
}