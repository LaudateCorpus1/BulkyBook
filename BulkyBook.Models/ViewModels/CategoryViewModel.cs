using System.Collections.Generic;

namespace BulkyBook.Models.ViewModels
{
    public class CategoryViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
