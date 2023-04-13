using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner)]
    public class ServiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ApplicationUser user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }

            InsertLog("View Treatment Service List", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TreatmentVM obj)
        {
            if (ModelState.IsValid)
            {
                var treatmentList = _unitOfWork.Treatment.GetAll().Where(x => x.ClinicId == obj.ClinicId);

                if (!treatmentList.Any(x => x.Name == obj.Treatment.Name))
                {
                    _unitOfWork.Treatment.Add(obj.Treatment);
                    _unitOfWork.Save();

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Create Treatment Service", user.Id, user.ClinicId, SD.AuditCreate);

                    TempData["success"] = "Treatment created successfully";
                }
                else
                {
                    TempData["error"] = "The treatment is already existing.";
                }
                
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(TreatmentVM obj)
        {
            if (ModelState.IsValid)
            {
                var treatmentList = _unitOfWork.Treatment.GetAll().Where(x => x.ClinicId == obj.ClinicId);

                if(!treatmentList.Any(x => x.Name == obj.Treatment.Name))
                {
                    _unitOfWork.Treatment.Update(obj.Treatment);
                    _unitOfWork.Save();

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Update Treatment Service", user.Id, user.ClinicId, SD.AuditUpdate);

                    TempData["success"] = "Treatment updated successfully";
                }
                else
                {
                    TempData["error"] = "The treatment is already existing.";
                }
                
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        #region API CALLS
        [HttpGet]
        public string GetAll(string status)
        {
            var user = GetCurrentUser();
            var treatmentList = _unitOfWork.Treatment.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "TreatmentType");
            switch (status)
            {
                case "archived":
                    treatmentList = treatmentList.Where(x => x.IsArchived == true).ToList();
                    break;
                case "active":
                    treatmentList = treatmentList.Where(x => x.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(treatmentList, jsonSettings);
        }

        [HttpGet]
        public IActionResult GetTreatment(int id)
        {
            Treatment treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == id, includeProperties: "TreatmentType");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Treatment Service", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewTreatment", treatment);
        }

        [HttpGet]
        public IActionResult LoadAddForm()
        {
            ApplicationUser user = GetCurrentUser();

            TreatmentVM treatmentVM = new TreatmentVM()
            {
                Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId),
                Treatment = new(),
                TreatmentTypeList = _unitOfWork.TreatmentType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Type,
                    Value = i.Id.ToString(),
                })
            };

            InsertLog("View Add Treatment Service Form", user.Id, user.ClinicId, SD.AuditCreate);

            return PartialView("_AddTreatmentForm", treatmentVM);
        }

        [HttpGet]
        public IActionResult LoadEditForm(int id)
        {
            ApplicationUser user = GetCurrentUser();

            TreatmentVM treatmentVM = new TreatmentVM()
            {
                Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId),
                Treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == id),
                TreatmentTypeList = _unitOfWork.TreatmentType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Type,
                    Value = i.Id.ToString()
                })
            };

            InsertLog("View Edit Treatment Service Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditTreatmentForm", treatmentVM);
        }

        [HttpPost]
        public IActionResult Archive(int? id)
        {
            var treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == id, includeProperties: "TreatmentType");
            if (treatment == null)
            {
                return Json(new { success = false, message = "Treatment does not exist." });
            }
            treatment.LastModified = DateTime.Now;
            treatment.IsArchived = true;
            _unitOfWork.Treatment.Update(treatment);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Archive Treatment Service", user.Id, user.ClinicId, SD.AuditArchive);

            return Json(new { success = true, message = "Archived successfully!" });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == id, includeProperties: "TreatmentType");
            if (treatment == null)
            {
                return Json(new { success = false, message = "Treatment does not exist." });
            }
            _unitOfWork.Treatment.Remove(treatment);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Delete Treatment Service", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Deleted successfully!" });
        }
        #endregion

        #region HELPER FUNCTIONS

        public void InsertLog(string activityType, string userId, int? clinicId, string description)
        {
            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = activityType,
                DateAdded = DateTime.Now,
                UserId = userId,
                ClinicId = clinicId,
                Description = description,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();
        }

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string? userid = claim.Value;
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userid);
        }

        #endregion
    }
}