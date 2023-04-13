using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
    public class TreatmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TreatmentController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index(int id)
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
            InsertLog("View Treatment Record List", user.Id, user.ClinicId, SD.AuditView);

            ViewData["CurrentPage"] = "treatmentRecord";

            return View();
        }

        [HttpPost]
        public IActionResult CreateTreatment(int patientId)
        {
            ApplicationUser user = GetCurrentUser();

            TreatmentRecord record = new TreatmentRecord();
            record.PatientId = patientId;
            record.ClinicId = (int)user.ClinicId;
            
            _unitOfWork.TreatmentRecord.Add(record);
            _unitOfWork.Save();
                        
            InsertLog("Create Treatment Record Draft", user.Id, user.ClinicId, SD.AuditCreate);

            return RedirectToAction("Upsert", new { id = record.Id });
        }

        public IActionResult Upsert(int? id)
        {
            ViewData["CurrentPage"] = "treatmentRecord";
            
            var record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);

            if (record.TreatmentId == null)
            {
                return View(record);
            }

            record.Treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == record.TreatmentId, includeProperties: "TreatmentType");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Treatment Record Upsert", user.Id, user.ClinicId, SD.AuditView);

            return View(record);
        }

        [HttpPost]
        public IActionResult AddToInvoice(int recordId)
        {
            var record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == recordId);
            record.DateCreated = DateTime.Now;

            if(record.TreatmentId == null)
            {
                TempData["error"] = "A treatment must be selected to save";
                return RedirectToAction("Upsert", new { id = recordId });
            }
            else if (record.Dentists == null)
            {
                TempData["error"] = "A Dentist must be selected to save";
                return RedirectToAction("Upsert", new { id = recordId });
            }

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            TempData["success"] = "The treatment record was successfully created.";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Create Finalized Treatment Record", user.Id, user.ClinicId, SD.AuditCreate);

            return RedirectToAction("Index", new { id = record.PatientId });
        }

        [HttpPost]
        public IActionResult DiscardTreatmentRecord(int recordId)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == recordId);
            int? patientId = record.PatientId;

            _unitOfWork.TreatmentRecord.Remove(record);
            _unitOfWork.Save();

            TempData["success"] = "Successfully discarded treatment record.";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Discard Treatment Record", user.Id, user.ClinicId, SD.AuditDelete);

            return RedirectToAction("Index", new { id = patientId });
        }

        #region API CALLS

        public string GetAllTreatmentRecords(int id, string status)
        {
            var list = _unitOfWork.TreatmentRecord.GetAll(x => x.PatientId == id, includeProperties: "Treatment");

            if (status == "archived")
                list = list.Where(x => x.IsArchived == true);
            else
                list = list.Where(x => x.IsArchived == false);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(list, jsonSettings);
        }

        [HttpGet]
        public IActionResult LoadTreatment(int id)
        {
            var treatment = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id, includeProperties: "Treatment");

            if (treatment == null) return NotFound();

            var vm = new TreatmentRecordVM();
            vm.TreatmentRecord = treatment;

            if (treatment.DateCreated != null)
            {
                DateTime dateCreated = (DateTime)treatment.DateCreated;
                vm.DateCreated = dateCreated.ToString("MM/dd/yyyy hh:mm tt");
            } 
            else
            {
                vm.DateCreated = "N/A";
            }

            if (treatment.LastModified != null)
            {
                DateTime lastModified = (DateTime)treatment.LastModified;
                vm.LastModified = lastModified.ToString("MM/dd/yyyy hh:mm tt");
            }
            else
            {
                vm.LastModified = "N/A";
            }

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Treatment Record", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewTreatment", vm);
        }

        [HttpGet]
        public IActionResult LoadAddTreatmentForm(int id)
        {
            ViewData["Id"] = id;

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Add Treatment Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_AddTreatmentForm");
        }

        [HttpGet]
        public IActionResult GetAllTreatments()
        {
            ApplicationUser user = GetCurrentUser();
            var treatmentList = _unitOfWork.Treatment.GetAll(includeProperties: "TreatmentType").Where(x => x.IsArchived == false && x.ClinicId == user.ClinicId);
            return Json(new { data = treatmentList });
        }

        [HttpPost]
        public void AddTreatment(int treatmentRecordId, int treatmentId)
        {
            Treatment treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == treatmentId);

            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == treatmentRecordId);
            record.TreatmentId = treatmentId;
            record.TotalPrice = treatment.Price;
            record.Quantity = 1;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Add Treatment To Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);
        }

        [HttpPost]
        public void IncrementQuantity(int id)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            Treatment? treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == record.TreatmentId);

            record.Quantity += 1;
            record.TotalPrice = treatment.Price * record.Quantity;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            TempData["success"] = "The quantity was successfully increased.";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Increased Quantity", user.Id, user.ClinicId, SD.AuditUpdate);
        }

        [HttpPost]
        public void DecrementQuantity(int id)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            Treatment? treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == record.TreatmentId);

            if (record.Quantity == 1)
            {
                TempData["error"] = "The quantity cannot be decreased any further.";
            }
            else
            {
                record.Quantity -= 1;
                record.TotalPrice = treatment.Price * record.Quantity;

                _unitOfWork.TreatmentRecord.Update(record);
                _unitOfWork.Save();

                TempData["success"] = "The quantity was successfully reduced.";

                ApplicationUser user = GetCurrentUser();
                InsertLog("Decreased Quantity", user.Id, user.ClinicId, SD.AuditUpdate);
            }
        }

        [HttpGet]
        public IActionResult LoadAddToothForm(int id)
        {
            ViewData["Id"] = id;

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Add Tooth Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_AddToothForm");
        }

        [HttpPost]
        public void AddTooth(int id, string[] data)
        {
            string tooth = "";

            for (int i = 0; i < data.Length; i++)
            {
                if (i == data.Length - 1)
                    tooth = AppendString(tooth, data[i], true);
                else
                    tooth = AppendString(tooth, data[i]);
            }

            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            record.ToothNumbers = tooth;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            TempData["success"] = "Tooth numbers have been added to the record.";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Add Tooth to Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);
        }

        [HttpPost]
        public void AddTeeth(int id, string data)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            record.ToothNumbers = data;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            TempData["success"] = "Tooth numbers have been added to the record.";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Add Tooth to Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> LoadAddDentistFormAsync(int id)
        {
            ApplicationUser? currentUser = GetCurrentUser();

            List<ApplicationUser>? clinicUsers = _userManager.Users.Where(x => x.ClinicId == currentUser.ClinicId && x.IsArchived==false).ToList();

            List<ApplicationUser>? dentists = new List<ApplicationUser>();

            foreach (ApplicationUser user in clinicUsers)
            {
                if ((await _userManager.IsInRoleAsync(user, SD.Role_Dentist)) || (await _userManager.IsInRoleAsync(user, SD.Role_Owner)))
                    dentists.Add(new ApplicationUser()
                    {
                        Id = user.Id,
                        FirstName = AesOperation.DecryptString(user.FirstName),
                        LastName = AesOperation.DecryptString(user.LastName),
                    });
            }

            ViewData["Id"] = id;

            InsertLog("View Add Dentist Form", currentUser.Id, currentUser.ClinicId, SD.AuditView);

            return PartialView("_AddDentistForm", dentists);
        }

        [HttpPost]
        public void AddDentists(int id, string[] data)
        {
            string dentists = "";
            if(data.Length != 0) { 
                for (int i = 0; i < data.Length; i++)
                {
                    if (i == data.Length - 1)
                        dentists = AppendString(dentists, data[i], true);
                    else
                        dentists = AppendString(dentists, data[i]);
                }

                TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
                record.Dentists = dentists;

                _unitOfWork.TreatmentRecord.Update(record);
                _unitOfWork.Save();

                TempData["success"] = "Dentist/s have been added to the record.";

                ApplicationUser user = GetCurrentUser();
                InsertLog("Add Dentist to Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);
            }
            else
            {
                TempData["error"] = "At least one dentist must be selected";
            }
           
        }

        [HttpPost]
        public JsonResult Archive(int id)
        {
            var obj = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            
            if (obj == null)
                return Json(new { success = false, message = "Treatment record does not exist." });

            obj.IsArchived = true;
            _unitOfWork.TreatmentRecord.Update(obj);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Archived Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);

            return Json(new { success = true, message = "Archived successfully!" });
        }

        [HttpDelete]
        public JsonResult Delete(int id)
        {
            var obj = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            
            if (obj == null)
                return Json(new { success = false, message = "Treatment record does not exist." });
            
            _unitOfWork.TreatmentRecord.Remove(obj);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Deleted Treatment Record", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

        #region HELPER FUNCTIONS

        public string AppendString(string mainString, string stringToAppend, bool lastSequence = false)
        {
            if (lastSequence)
                return mainString + stringToAppend;

            return mainString + stringToAppend + ", ";
        }

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
        }

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

        #endregion

    }
}
