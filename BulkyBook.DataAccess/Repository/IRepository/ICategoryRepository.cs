using BulkyBook.Models;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepositoryAsync<Category>
    {
        Task UpdateAsync(Category entity);
    }
}
