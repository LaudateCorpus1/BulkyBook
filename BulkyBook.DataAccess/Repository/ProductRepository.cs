using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Product entity)
        {
            var product = base.GetFirstOrDefault(a => a.Id == entity.Id);

            if (product != null)
            {
                if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
                    product.ImageUrl = entity.ImageUrl;

                product.ISBN = entity.ISBN;
                product.ListPrice = entity.ListPrice;
                product.Price = entity.Price;
                product.Price100 = entity.Price100;
                product.Price50 = entity.Price50;
                product.Title = entity.Title;
                product.Author = entity.Author;
                product.CategoryId = entity.CategoryId;
                product.CoverTypeID = entity.CoverTypeID;
                product.Description = entity.Description;
            }
        }
    }
}
