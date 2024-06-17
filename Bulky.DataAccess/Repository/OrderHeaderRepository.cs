using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db) => _db = db;

        public void Save()
        {
            Db.SaveChanges();
        }

        public void Update(OrderHeader orderHeader)
        {
            Db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader? orderHeader = Db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (orderHeader == null) return;

            
            orderHeader.OrderStatus = orderStatus;
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orderHeader.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            OrderHeader? orderHeader = Db.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (orderHeader == null) return;

            if (!string.IsNullOrEmpty(sessionId))
            {
                orderHeader.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderHeader.PaymentIntentId = paymentIntentId;
                orderHeader.PaymentDate = DateTime.Now;
            }
        }
    }
}
