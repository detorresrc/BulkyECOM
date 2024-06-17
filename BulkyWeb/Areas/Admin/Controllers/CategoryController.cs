using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private ICategoryRepository Category => _unitOfWork.Category;

        public CategoryController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public IActionResult Index()
        {
            List<Category> categories = Category.GetAll().ToList();

            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                Category.Add(category);
                Category.Save();

                TempData["success"] = "Category created successfully";

                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? categoryId)
        {
            Category category = Category.Get(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int? categoryId, Category category)
        {
            if (ModelState.IsValid)
            {
                Category.Update(category);
                Category.Save();

                TempData["success"] = "Category updated successfully";

                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? categoryId)
        {
            Category category = Category.Get(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? categoryId)
        {
            Category category = Category.Get(c => c.Id == categoryId);
            if (category == null)
            {
                return NotFound();
            }

            Category.Remove(category);
            Category.Save();

            TempData["success"] = "Category delete successfully";

            return RedirectToAction("Index");
        }
    }
}
