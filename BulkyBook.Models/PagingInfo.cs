using System;

namespace BulkyBook.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public string urlParam { get; set; }
        public int TotalPage => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}
