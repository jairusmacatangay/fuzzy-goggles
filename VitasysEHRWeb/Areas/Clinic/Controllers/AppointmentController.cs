using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
    public class AppointmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ApplicationUser user = GetCurrentUser();
            if (AuthorizeAccess() == false) return View("Error");
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }

            InsertLog("View Appointment List", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDeny(int id, string status, string notes)
        {
            var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == id, includeProperties:"AppointmentTime,Patient,Clinic");
            ApplicationUser user = GetCurrentUser();
            EmailSender emailSender = new EmailSender();

            if (appointment == null)
            {
                TempData["error"] = "Appointment does not exist.";
                return RedirectToAction("Index");
            }

            if(status == "Approve")
            {
                appointment.AppointmentStatusId = 1;
                appointment.Notes = notes;

                await emailSender.SendEmailAsync(
                    appointment.Patient.Email,
                    "Appointment #" + appointment.Id ,
                    $"Your appointment has been <b>approved</b>.<br/><br/> " +
                    $"Please be here in {AesOperation.DecryptString(appointment.Clinic.Name)} at <b>{appointment.AppointmentDate.DayOfWeek}, {appointment.AppointmentTime.TimeSlot} on {appointment.AppointmentDate.Month}/{appointment.AppointmentDate.Day}/{appointment.AppointmentDate.Year}</b>.<br/><br/>" +
                    $"{(notes == null ? "" : $"<b>Notes from {AesOperation.DecryptString(appointment.Clinic.Name)}:</b><br/>{appointment.Notes}" )} <br/><br/>" +
                    $"If you have any concerns, contact us at: <br/>" +
                    $"Email: {AesOperation.DecryptString(appointment.Clinic.EmailAddress)} <br/>" +
                    $"Mobile Number: {AesOperation.DecryptString(appointment.Clinic.MobilePhone)} <br/>" +
                    $"Office Number: {AesOperation.DecryptString(appointment.Clinic.OfficePhone)} <br/>"
                    );

                TempData["success"] = "Appointment was successfully approved.";
                InsertLog("Approved Appointment", user.Id, user.ClinicId, SD.AuditUpdate);
            }
            else
            {
                appointment.AppointmentStatusId = 3;
                appointment.Notes = notes;

                await emailSender.SendEmailAsync(
                    appointment.Patient.Email,
                    "Appointment #" + appointment.Id,
                    $"Your appointment has been <b>denied</b>.<br/><br/> " +
                    $"{(notes == null ? "" : $"<b>Notes from {AesOperation.DecryptString(appointment.Clinic.Name)}:</b><br/>{appointment.Notes}")} <br/><br/>" +
                    $"If you have any concerns, contact us at: <br/>" +
                    $"Email: {AesOperation.DecryptString(appointment.Clinic.EmailAddress)} <br/>" +
                    $"Mobile Number: {AesOperation.DecryptString(appointment.Clinic.MobilePhone)} <br/>" +
                    $"Office Number: {AesOperation.DecryptString(appointment.Clinic.OfficePhone)} <br/>"
                    );

                TempData["success"] = "Appointment was successfully denied.";
                InsertLog("Denied Appointment", user.Id, user.ClinicId, SD.AuditUpdate);
            }

            appointment.LastModified = DateTime.Now;

            _unitOfWork.Appointment.Update(appointment);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        #region API CALLS

        public string GetAllAppointments()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();

            IEnumerable<Appointment>? list = _unitOfWork.Appointment.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "AppointmentTime,Patient,AppointmentStatus");
            List<AppointmentListVM> appointments = new List<AppointmentListVM>();


            foreach (var item in list)
            {
                appointments.Add(new AppointmentListVM()
                {
                    Id = item.Id,
                    AppointmentDate = item.AppointmentDate,
                    AppointmentTime = item.AppointmentTime,
                    AppointmentStatus = item.AppointmentStatus.Status,
                    Patient = AesOperation.DecryptString(item.Patient.LastName) + ", " + AesOperation.DecryptString(item.Patient.FirstName) + " " + AesOperation.DecryptString(item.Patient.MiddleName)
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(appointments, jsonSettings);
        }

        public IActionResult LoadAppointment(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == id, includeProperties: "AppointmentTime,Patient,AppointmentStatus");

            if (appointment == null)
            {
                TempData["error"] = "Appointment does not exist.";
                return RedirectToAction("Index");
            }

            var vm = new AppointmentVM()
            {
                Appointment = appointment,
                AppointmentDate = appointment.AppointmentDate.ToString("MM/dd/yyyy"),
                Patient = AesOperation.DecryptString(appointment.Patient.LastName) + ", " + AesOperation.DecryptString(appointment.Patient.FirstName) + " " + AesOperation.DecryptString(appointment.Patient.MiddleName)
            };

            DateTime lastModified;

            if (appointment.LastModified != null)
            {
                lastModified = (DateTime)appointment.LastModified;
                vm.LastModified = lastModified.ToString("MM/dd/yyyy hh:mm:ss tt");

                return PartialView("_ViewAppointment", vm);
            }

            vm.LastModified = "N/A";

            var user = GetCurrentUser();
            InsertLog("View Appointment", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewAppointment", vm);
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

            if (subscriptionType == "Free" || subscriptionType == "Basic" || subscriptionIsLockout == "true")
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
