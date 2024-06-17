using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db) => _db = db;

        public void Save()
        {
            Db.SaveChanges();
        }

        public void AddItem(ShoppingCart shoppingCart)
        {
            ShoppingCart? existingProductFromShoppingcart = Db.ShoppingCarts
                .FirstOrDefault(s => s.ProductId == shoppingCart.ProductId && s.ApplicationUserId == shoppingCart.ApplicationUserId);
            if(existingProductFromShoppingcart == null)
            {
                Db.ShoppingCarts.Add(shoppingCart);
            }
            else
            {
                existingProductFromShoppingcart.Count += shoppingCart.Count;
                this.Update(existingProductFromShoppingcart);
            }

            this.Save();
        }

        public void Update(ShoppingCart shoppingCart)
        {
            Db.ShoppingCarts.Update(shoppingCart);
        }

        public IEnumerable<ShoppingCart> GetCartList(string userId)
        {
            IEnumerable<ShoppingCart> shoppingCart = Db.ShoppingCarts.Include(sc => sc.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(sc => sc.ApplicationUserId == userId);

            return shoppingCart.ToList();
        }
    }
}
