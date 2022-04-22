using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess.Repository.IRepository;

namespace MyBook.ViewComponents
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly IUnitofWork _unitOfWork;

        public UserNameViewComponent(IUnitofWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claims.Value);

            return View(userFromDb);
        }
    }
}
