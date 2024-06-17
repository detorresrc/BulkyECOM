using BulkyWebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;

        [BindProperty]
        public Category? Category { get; set; }

        public EditModel(ApplicationDbContext db) => _db = db;

        public void OnGet(int? categoryId)
        {
            Category = Db.Categories.FirstOrDefault(c => c.Id == categoryId);
        }

        public IActionResult OnPost()
        {
            if (Category == null)
            {
                return NotFound("Category not found!");
            }
            Db.Categories.Update(Category);
            Db.SaveChanges();

            return RedirectToPage("Index");
        }
    }
}
