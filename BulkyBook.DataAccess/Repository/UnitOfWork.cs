using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            Category = new CategoryRepository(_dbContext);
            CoverType = new CoverTypeRepository(_dbContext);
            StoredProcedureCall = new StoredProcedureCall(_dbContext);
            Product = new ProductRepository(_dbContext);
            Company = new CompanyRepository(_dbContext);
            ApplicationUser = new ApplicationUserRepository(_dbContext);
            OrderDetails = new OrderDetailsRepository(_dbContext);
            OrderHeader = new OrderHeaderRepository(_dbContext);
            ShoppingCart = new ShoppingCartRepository(_dbContext);
        }

        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public IStoredProcedureCall StoredProcedureCall { get; private set; }
        public ICompanyRepository Company { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
