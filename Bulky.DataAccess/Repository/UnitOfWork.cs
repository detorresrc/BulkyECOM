using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork(ApplicationDbContext db) : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; } = new CategoryRepository(db);
        public IProductRepository Product { get; private set; } = new ProductRepository(db);
        public IProductImageRepository ProductImage { get; private set; } = new ProductImageRepository(db);
        public ICompanyRepository Company { get; private set; } = new CompanyRepository(db);
        public IShoppingCartRepository ShoppingCart { get; set; } = new ShoppingCartRepository(db);
        public IApplicationUserRepository ApplicationUser { get; private set; } = new ApplicationUserRepository(db);
        public IOrderHeaderRepository OrderHeader { get; set; } = new OrderHeaderRepository(db);
        public IOrderDetailRepository OrderDetail { get; set; } = new OrderDetailRepository(db);

        public void Save()
        {
            db.SaveChanges();
        }
        
        public IDbContextTransaction BeginTransaction()
        {
            return db.Database.BeginTransaction();
        }
    }
}
