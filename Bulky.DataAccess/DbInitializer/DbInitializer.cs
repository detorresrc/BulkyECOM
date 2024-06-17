using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bulky.DataAccess.DbInitializer;

public class DbInitializer(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext db,
    ILogger<DbInitializer> logger) : IDbInitializer
{
    public void Initialize()
    {
        try
        {
            if(db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }

        if (!roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
            
            userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                Name = "Rommel de Torres",
                EmailConfirmed = true,
                PhoneNumber = "123321",
                StreetAddress = "Address",
                State = "IL",
                PostalCode = "23321",
                City = "Bulacan"
            }, "Admin123*").GetAwaiter().GetResult();

            ApplicationUser user = db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
            userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
        }
    }
}