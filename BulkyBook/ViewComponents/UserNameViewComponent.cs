using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _uow;

        public UserNameViewComponent(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userFromDB = _uow.ApplicationUser.GetFirstOrDefault(a => a.Id == claims.Value);

            return View(userFromDB);
        }
    }
}
