using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
    public class ReminderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReminderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }
            InsertLog("View Reminder List", user.Id, user.ClinicId, SD.AuditView);
            ViewData["CurrentPage"] = "reminders";
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();
            InsertLog("View Create Reminder", user.Id, user.ClinicId, SD.AuditView);
            ViewData["CurrentPage"] = "reminders";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(Reminder model)
        {
            if(ModelState.IsValid)
            {
                var user = GetCurrentUser();

                Reminder reminder = new Reminder();
                reminder.Title = model.Title;
                reminder.Content = model.Content;
                reminder.ClinicId = (int)user.ClinicId;
                reminder.PatientId = model.PatientId;
                reminder.DateAdded = model.DateAdded;

                _unitOfWork.Reminder.Add(reminder);
                _unitOfWork.Save();

                //Send email
                var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == reminder.PatientId);
                EmailSender emailSender = new EmailSender();
                await emailSender.SendEmailAsync(patient.Email, reminder.Title, reminder.Content);

                InsertLog("Create Reminder", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Reminder was successfully created!";

                return RedirectToAction("Index");
            }
            
            ViewData["CurrentPage"] = "reminders";
            return View(model);
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();
            InsertLog("View Update Reminder", user.Id, user.ClinicId, SD.AuditUpdate);
            var reminder = _unitOfWork.Reminder.GetFirstOrDefault(x => x.Id == id);
            ViewData["CurrentPage"] = "reminders";
            return View(reminder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAsync(Reminder model)
        {
            if (ModelState.IsValid)
            {
                var reminder = _unitOfWork.Reminder.GetFirstOrDefault(x => x.Id == model.Id);
                reminder.Title = model.Title;
                reminder.Content = model.Content;
                reminder.LastModified = DateTime.Now;

                _unitOfWork.Reminder.Update(reminder);
                _unitOfWork.Save();

                // Send email
                var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == reminder.PatientId);
                EmailSender emailSender = new EmailSender();
                await emailSender.SendEmailAsync(patient.Email, reminder.Title + " [Edited]", reminder.Content);

                var user = GetCurrentUser();
                InsertLog("Update Reminder", user.Id, user.ClinicId, SD.AuditUpdate);

                TempData["success"] = "Successfully updated reminder!";

                return RedirectToAction("Index");
            }

            ViewData["CurrentPage"] = "reminders";
            return View(model);
        }

        #region API CALLS

        public string GetAllReminders(int id)
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();
            
            var reminders = _unitOfWork.Reminder.GetAll(x => x.PatientId == id && x.ClinicId == user.ClinicId);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(reminders, jsonSettings);
        }

        public IActionResult LoadReminder(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var reminder = _unitOfWork.Reminder.GetFirstOrDefault(x => x.Id == id);

            if (reminder == null) return NotFound();

            var user = GetCurrentUser();
            InsertLog("View Reminder", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewReminder", reminder);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var reminder = _unitOfWork.Reminder.GetFirstOrDefault(x => x.Id == id);

            if (reminder == null) 
                return Json(new { success = false, message = "Reminder does not exist." });

            _unitOfWork.Reminder.Remove(reminder);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Delete Reminder", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Reminder successfully deleted!" });
        }

        #endregion

        #region HELPER FUNCTIONS

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
        public bool AuthorizeAccess()
        {
            string? subscriptionType = HttpContext.Session.GetString(SD.SessionKeySubscriptionType);
            string? subscriptionIsLockout = HttpContext.Session.GetString(SD.SessionKeySubscriptionIsLockout);

            if (subscriptionType == "Free" || subscriptionIsLockout == "true")
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
