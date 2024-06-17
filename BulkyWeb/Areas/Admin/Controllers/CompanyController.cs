using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private ICompanyRepository Company => _unitOfWork.Company;

        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? companyId)
        {
            Company company = new Company();

            if (!(companyId == null || companyId == 0))
            {
                company = this.Company.Get(c => c.Id == companyId);
            }

            return View(company);
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                string message;
                if(company.Id == 0)
                {
                    this.Company.Add(company);
                    message = "Company created successfully";
                }
                else
                {
                    this.Company.Update(company);
                    message = "Company updated successfully";
                }
                    
                this.Company.Save();

                TempData["success"] = message;

                return RedirectToAction("Index");
            }

            return View(company);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = this.Company.GetAll().ToList();

            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company company = this.Company.Get(c => c.Id == id);
            if (company == null)
            {
                return Json(new { success = true, message = "Company not found!" });
            }

            this.Company.Remove(company);
            this.Company.Save();

            return Json(new { success = true, message = "Company deleted successfully"});
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
