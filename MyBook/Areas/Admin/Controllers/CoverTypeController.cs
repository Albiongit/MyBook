using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess.Repository.IRepository;
using MyBook.Models;
using MyBook.Utility;

namespace MyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitofWork _unitOfWork;

        public CoverTypeController(IUnitofWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();

            if(id == null)
            {
                // create
                return View(coverType);
            }
            else
            {
                // coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());
                var parameter = new DynamicParameters();
                parameter.Add("@Id", id);
                coverType = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);
                
                if(coverType == null)
                {
                    return NotFound();
                }
                return View(coverType); 
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Name", coverType.Name);
                if(coverType.Id == 0)
                {
                    //create
                    //_unitOfWork.CoverType.Add(coverType);
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Create, parameter);
                }
                else
                {
                    parameter.Add("@Id", coverType.Id);
                    //update
                    //_unitOfWork.CoverType.Update(coverType);
                    _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Update, parameter);   
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(coverType);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            //var allObj = _unitOfWork.CoverType.GetAll();
            var allObj = _unitOfWork.SP_Call.List<CoverType>(SD.Proc_CoverType_GetAll, null);
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);
            
            var objFromDb = _unitOfWork.SP_Call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);
            //var objFromDb = _unitOfWork.CoverType.Get(id);

            if (objFromDb == null)
            {
                TempData["Success"] = "Error deleting CoverType";
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                _unitOfWork.SP_Call.Execute(SD.Proc_CoverType_Delete, parameter);
               // _unitOfWork.CoverType.Remove(objFromDb);
                _unitOfWork.Save();

                TempData["Success"] = "CoverType successfully deleted";
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion

    }
}
