using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);

        void AddItem(ShoppingCart shoppingCart);
        void Save();
        IEnumerable<ShoppingCart> GetCartList(string userId);
    }
}
