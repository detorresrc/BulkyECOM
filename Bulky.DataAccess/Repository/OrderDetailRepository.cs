using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db) => _db = db;

        public void Save()
        {
            Db.SaveChanges();
        }

        public void Update(OrderDetail orderDetail)
        {
            Db.OrderDetails.Update(orderDetail);
        }
    }
}
