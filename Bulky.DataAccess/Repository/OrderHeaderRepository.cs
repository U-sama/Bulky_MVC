using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
        }
        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			OrderHeader orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
			if (orderFromDb != null)
			{
				orderFromDb.OrderStatus = orderStatus;
				if(paymentStatus != null)
					orderFromDb.PaymentStatus = paymentStatus;
			}
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			OrderHeader orderFromDb = _db.OrderHeaders.FirstOrDefault(o => o.Id == id);
			if (orderFromDb != null)
			{
				if (!string.IsNullOrEmpty(sessionId))
				{
					orderFromDb.SessionId = sessionId;
				}
				if(!string.IsNullOrEmpty(paymentIntentId))
				{
					orderFromDb.PaymentIntentId = paymentIntentId;
					orderFromDb.PaymentDate = DateTime.Now;
				}
			}
		}
	}
}
