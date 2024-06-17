using System.Security.Claims;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.ViewComponents;

public class ShoppingCartViewComponent(IUnitOfWork unitOfWork) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            int? count =
                    (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                        ? HttpContext.Session.GetInt32(SD.SessionCart)
                        :
                        unitOfWork.ShoppingCart
                            .GetAll(sc => sc.ApplicationUserId == userId)
                            .Count();
            
            HttpContext.Session.SetInt32(
                SD.SessionCart, count ?? 0
            );
            
            return View(count ?? 0);
        }

        HttpContext.Session.Clear();
        return View(0);
    }
}