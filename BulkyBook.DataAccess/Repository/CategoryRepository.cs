using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public void Update(Category entity)
        {
            var category = base.GetFirstOrDefault(a => a.Id == entity.Id);

            if (category != null)
            {
                category.Name = entity.Name;
            }
        }
    }
}
