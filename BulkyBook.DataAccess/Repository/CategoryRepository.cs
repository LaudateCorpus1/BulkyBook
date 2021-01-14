using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : RepositoryAsync<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task UpdateAsync(Category entity)
        {
            var category = await base.GetFirstOrDefaultAsync(a => a.Id == entity.Id);

            if (category != null)
            {
                category.Name = entity.Name;
            }
        }
    }
}
