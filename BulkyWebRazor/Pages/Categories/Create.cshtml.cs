using BulkyWebRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;

        [BindProperty]
        public Category Category { get; set; }

        public CreateModel(ApplicationDbContext db) => _db = db;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            Db.Categories.Add(Category);
            Db.SaveChanges();

            return RedirectToPage("Index");
        }
    }
}
