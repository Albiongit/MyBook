using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess.Repository.IRepository;
using MyBook.Models;
using MyBook.Models.ViewModels;
using MyBook.Utility;
using System.Linq;
using System.Threading.Tasks;

namespace MyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _unitOfWork;

        public CategoryController(IUnitofWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }

        public async Task<IActionResult> Index(int productPage = 1)
        {
            CategoryVM categoryVM = new CategoryVM()
            {
                Categories = await _unitOfWork.Category.GetAllAsync()
        };

            var count = categoryVM.Categories.Count();
            categoryVM.Categories = categoryVM.Categories.OrderBy(c => c.Name).ToList();

            categoryVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = 10,
                TotalItem = count,
                urlParam = "/Admin/Category/Index?productPage=:"
            };

            return View(categoryVM);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            Category category = new Category();
            if(id == null)
            {
                return View(category);
            }

            category = await _unitOfWork.Category.GetAsync(id.GetValueOrDefault());
            if(category == null)
            {
                return NotFound();
            }
            return View(category);

       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  
        public async Task<IActionResult> Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.Id == 0)
                {
                   await _unitOfWork.Category.AddAsync(category);
                
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index)); 
            }
            return View(category);
        }







        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var allObj = await _unitOfWork.Category.GetAllAsync();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.Category.GetAsync(id);
            if(objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                await _unitOfWork.Category.RemoveAsync(objFromDb);
                _unitOfWork.Save();

                TempData["Success"] = "Category successfully deleted";
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion
    }
}
