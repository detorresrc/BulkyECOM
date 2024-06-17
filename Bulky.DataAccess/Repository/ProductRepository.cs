using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;
        public ProductRepository(ApplicationDbContext db) : base(db) => _db = db;

        public void Save()
        {
            Db.SaveChanges();
        }

        public void Update(Product product)
        {
            Db.Products.Update(product);
        }
    }
}
