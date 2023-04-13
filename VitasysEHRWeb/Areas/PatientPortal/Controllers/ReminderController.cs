using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientPortal.Controllers
{
    [Area("PatientPortal")]
    public class ReminderController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public ReminderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null) return RedirectToAction("Login", "Account");

            InsertLog("View Reminder List", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "reminders";
            return View();
        }

        public string GetAllReminders()
        {
            Patient? user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null) return String.Empty;

            var list = _unitOfWork.Reminder.GetAll(x => x.PatientId == user.Id, includeProperties: "Clinic");
            List<ReminderListVM> reminders = new List<ReminderListVM>();

            foreach (var item in list)
            {
                reminders.Add(new ReminderListVM()
                {
                    Id = item.Id,
                    Title = item.Title,
                    Clinic = AesOperation.DecryptString(item.Clinic.Name),
                    DateAdded = item.DateAdded,
                    LastModified = item.LastModified
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(reminders, jsonSettings);
        }

        public IActionResult LoadReminder(int id)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null) return RedirectToAction("Login", "Account");

            var reminder = _unitOfWork.Reminder.GetFirstOrDefault(x => x.Id == id);

            if (reminder == null) return NotFound();

            InsertLog("View Reminder", user.Id, SD.AuditView);

            return PartialView("_ViewReminder", reminder);
        }

        public void InsertLog(string activityType, int patientId, string description)
        {
            _unitOfWork.AuditLogPatient.Add(new AuditLogPatient
            {
                ActivityType = activityType,
                DateAdded = DateTime.Now,
                PatientId = patientId,
                Description = description,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();
        }

    }
}
