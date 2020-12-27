using System;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Category { get; }
        ICoverTypeRepository CoverType { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IStoredProcedureCall StoredProcedureCall { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IShoppingCartRepository ShoppingCart { get; }
        void Save();
    }
}
