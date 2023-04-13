using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuditLogController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogController(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;

        }


        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Index()
        {
            ApplicationUser user = GetCurrentUser();
            InsertLog("View Audit Logs", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [Authorize(Roles = SD.Role_Owner)]
        public IActionResult Clinic()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Audit Logs", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        #region API Calls

        [HttpGet]
        [Authorize(Roles = SD.Role_Admin)]
        public string GetAuditLogs()
        {
            var logs = _unitOfWork.AuditLog.GetAll(includeProperties: "Clinic,ApplicationUser");
            
            List<AuditLogVM> decryptedLogs = new();

            foreach (var obj in logs)
            {
                if (obj.ApplicationUser == null) obj.ApplicationUser = new();

                if (obj.Clinic == null) obj.Clinic = new();

                string middleName = AesOperation.DecryptString(obj.ApplicationUser.MiddleName);
                
                decryptedLogs.Add(new AuditLogVM
                {
                    Id = obj.Id,
                    Username = AesOperation.DecryptString(obj.ApplicationUser.FirstName) + " " + AesOperation.DecryptString(obj.ApplicationUser.LastName),
                    Clinic = AesOperation.DecryptString(obj.Clinic.Name),
                    DateAdded = obj.DateAdded,
                    ActivityType = obj.ActivityType,
                    Description = obj.Description,
                    Device = obj.Device
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedLogs, jsonSettings);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner)]
        public string GetClinicAuditLogs()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            ApplicationUser user = GetCurrentUser();

            var logsFromDb = _unitOfWork.AuditLog.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "Clinic,ApplicationUser");

            List<AuditLogVM> decryptedLogs = new();

            foreach (var obj in logsFromDb)
            {
                decryptedLogs.Add(new AuditLogVM
                {
                    Id = obj.Id,
                    Username = AesOperation.DecryptString(obj.ApplicationUser.LastName) + ", " + AesOperation.DecryptString(obj.ApplicationUser.FirstName) + " " + AesOperation.DecryptString(obj.ApplicationUser.MiddleName),
                    DateAdded = obj.DateAdded,
                    ActivityType = obj.ActivityType,
                    Description = obj.Description,
                    Device = obj.Device
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedLogs, jsonSettings);
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

