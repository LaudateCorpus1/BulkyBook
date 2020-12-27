using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public OrderDetailsRepository(ApplicationDbContext dbContext) : base(dbContext) {
            _dbContext = dbContext;
        }

        public void Update(OrderDetails entity)
        {
            _dbContext.Update(entity);
            ////_dbContext.SaveChanges();
        }
    }
}
