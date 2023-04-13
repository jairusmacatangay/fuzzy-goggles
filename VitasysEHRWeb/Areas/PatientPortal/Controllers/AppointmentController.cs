using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Globalization;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientPortal.Controllers
{
    [Area("PatientPortal")]
    public class AppointmentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["CurrentPage"] = "appointments";

            InsertLog("View Appointments", user.Id, SD.AuditView);

            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["CurrentPage"] = "appointments";

            IEnumerable<ClinicPatient>? clinics = _unitOfWork.ClinicPatient.GetAll(x => x.PatientId == HttpContext.Session.GetInt32(SD.SessionKeyPatientId), includeProperties:"Clinic");

            List<VitasysEHR.Models.Clinic> clinicList = new();

            foreach (var item in clinics)
            {
                clinicList.Add(_unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == item.ClinicId));
            }

            AppointmentVM? vm = new AppointmentVM
            {
                TimeSlots = _unitOfWork.AppointmentTime.GetAll().Select(x => new SelectListItem
                {
                    Text = x.TimeSlot,
                    Value = x.Id.ToString(),
                }),
                Clinics = clinicList.Select(x => new SelectListItem
                {
                    Text = x.StartTime + "-" + x.EndTime + "-" + AesOperation.DecryptString(x.Name),
                    Value = x.Id.ToString(),
                }),
            };

            InsertLog("View Book Appointment", user.Id, SD.AuditView);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentVM vm)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var cultureInfo = new CultureInfo("en-PH");
                var appointmentDate = DateTime.Parse(vm.AppointmentDate, cultureInfo);

                var checkforExistingAppointment = _unitOfWork.Appointment.GetFirstOrDefault(x =>
                    x.AppointmentDate == appointmentDate &&
                    x.AppointmentTimeId == vm.Appointment.AppointmentTimeId &&
                    x.AppointmentStatusId != 3 &&
                    x.ClinicId == vm.Appointment.ClinicId);

                if (checkforExistingAppointment != null)
                {
                    vm.TimeSlots = _unitOfWork.AppointmentTime.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.TimeSlot,
                        Value = x.Id.ToString(),
                    });

                    IEnumerable<ClinicPatient>? clinics = _unitOfWork.ClinicPatient.GetAll(x => x.PatientId == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

                    List<VitasysEHR.Models.Clinic> clinicList = new();

                    foreach (var item in clinics)
                    {
                        clinicList.Add(_unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == item.ClinicId));
                    }

                    vm.Clinics = clinicList.Select(x => new SelectListItem
                    {
                        Text = AesOperation.DecryptString(x.Name),
                        Value = x.Id.ToString(),
                    });

                    var timeSlotList = _unitOfWork.AppointmentTime.GetAll();
                    var availableTimeSlot = new List<string>();

                    foreach (var item in timeSlotList)
                    {
                        var checkIfAvailableAppointment = _unitOfWork.Appointment.GetFirstOrDefault(x =>
                            x.AppointmentDate == appointmentDate &&
                            x.AppointmentTimeId == item.Id &&
                            x.AppointmentStatusId != 3 &&
                            x.ClinicId == vm.Appointment.ClinicId);

                        if (checkIfAvailableAppointment == null)
                        {
                            availableTimeSlot.Add(item.TimeSlot);
                        }
                    }

                    vm.AvailableTimeslots = availableTimeSlot;

                    ViewData["CurrentPage"] = "appointments";
                    TempData["ErrorMessage"] = "Appointment date and time slot is already taken";

                    return View(vm);
                }

                Appointment? appointment = new Appointment();
                appointment.PatientId = (int)HttpContext.Session.GetInt32(SD.SessionKeyPatientId);

                appointment.AppointmentDate = appointmentDate;
                appointment.Description = vm.Appointment.Description;
                appointment.AppointmentTimeId = vm.Appointment.AppointmentTimeId;
                appointment.ClinicId = vm.Appointment.ClinicId;
                appointment.AppointmentStatusId = 2;

                _unitOfWork.Appointment.Add(appointment);
                _unitOfWork.Save();

                TempData["AppointmentSuccess"] = "Your appointment was successfully created and is now marked as pending. Please wait for the clinic to approve it.";

                InsertLog("Book Appointment", user.Id, SD.AuditCreate);

                return RedirectToAction("Index");
            }

            return View(vm);
        }

        public IActionResult View(int id)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["CurrentPage"] = "appointments";

            var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == id, includeProperties: "AppointmentTime,Clinic,AppointmentStatus");

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment does not exist.";
                return RedirectToAction("Index");
            }

            var vm = new AppointmentVM();
            vm.Appointment = appointment;
            vm.Clinic = AesOperation.DecryptString(appointment.Clinic.Name);
            vm.AppointmentDate = appointment.AppointmentDate.ToString("MM/dd/yyyy");

            DateTime lastModified;

            if (appointment.LastModified != null)
            {
                lastModified = (DateTime)appointment.LastModified;
                vm.LastModified = lastModified.ToString("MM/dd/yyyy hh:mm:ss tt");

                return View(vm);
            }

            vm.LastModified = "N/A";

            InsertLog("View Appointment", user.Id, SD.AuditView);

            return View(vm);
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["CurrentPage"] = "appointments";

            var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == id, includeProperties: "AppointmentTime,Clinic,AppointmentStatus");

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment does not exist.";
                return RedirectToAction("Index");
            }

            var startTime = appointment.Clinic.StartTime;
            var endTime = appointment.Clinic.EndTime;
                
            var splitStartTime = startTime.Split("-");
            var startTimeIndex = splitStartTime[0];
            var startTimeValue = splitStartTime[1];

            var splitEndTime = endTime.Split("-");
            var endTimeIndex = splitEndTime[0];
            var endTimeValue = splitEndTime[1];

            var vm = new AppointmentVM
            {
                Appointment = appointment,
                AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                TimeSlots = _unitOfWork.AppointmentTime.GetAll().Select(x => new SelectListItem
                {
                    Text = x.TimeSlot,
                    Value = x.Id.ToString(),
                }),
                Clinic = AesOperation.DecryptString(appointment.Clinic.Name) + " (Clinic Hours: " + startTimeValue + "-" + endTimeValue + ")"
            };

            InsertLog("View Update Appointment", user.Id, SD.AuditView);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(AppointmentVM vm)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ModelState.ClearValidationState("Appointment.Description");
            ModelState.MarkFieldValid("Appointment.Description");

            if (ModelState.IsValid)
            {
                var cultureInfo = new CultureInfo("en-PH");
                var appointmentDate = DateTime.Parse(vm.AppointmentDate, cultureInfo);
                
                var checkforExistingAppointment = _unitOfWork.Appointment.GetFirstOrDefault(x =>
                    x.AppointmentDate == appointmentDate &&
                    x.AppointmentTimeId == vm.Appointment.AppointmentTimeId &&
                    x.AppointmentStatusId != 3 &&
                    x.ClinicId == vm.Appointment.ClinicId);

                if (checkforExistingAppointment != null)
                {
                    vm.TimeSlots = _unitOfWork.AppointmentTime.GetAll().Select(x => new SelectListItem
                    {
                        Text = x.TimeSlot,
                        Value = x.Id.ToString(),
                    });

                    IEnumerable<ClinicPatient>? clinics = _unitOfWork.ClinicPatient.GetAll(x => x.PatientId == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

                    List<VitasysEHR.Models.Clinic> clinicList = new();

                    foreach (var item in clinics)
                    {
                        clinicList.Add(_unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == item.ClinicId));
                    }

                    vm.Clinics = clinicList.Select(x => new SelectListItem
                    {
                        Text = AesOperation.DecryptString(x.Name),
                        Value = x.Id.ToString(),
                    });

                    var timeSlotList = _unitOfWork.AppointmentTime.GetAll();
                    var availableTimeSlot = new List<string>();

                    foreach (var item in timeSlotList)
                    {
                        var checkIfAvailableAppointment = _unitOfWork.Appointment.GetFirstOrDefault(x =>
                            x.AppointmentDate == appointmentDate &&
                            x.AppointmentTimeId == item.Id &&
                            x.AppointmentStatusId != 3 &&
                            x.ClinicId == vm.Appointment.ClinicId);

                        if (checkIfAvailableAppointment == null)
                        {
                            availableTimeSlot.Add(item.TimeSlot);
                        }
                    }

                    vm.AvailableTimeslots = availableTimeSlot;

                    ViewData["CurrentPage"] = "appointments";
                    TempData["ErrorMessage"] = "Appointment date and time slot is already taken";

                    return View(vm);
                }

                var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == vm.Appointment.Id);
                appointment.AppointmentTimeId = vm.Appointment.AppointmentTimeId;
                appointment.AppointmentDate = appointmentDate;
                appointment.AppointmentStatusId = 2;
                appointment.LastModified = DateTime.Now;

                _unitOfWork.Appointment.Update(appointment);
                _unitOfWork.Save();

                TempData["AppointmentSuccess"] = "Your appointment was successfully updated and is now marked as pending. Please wait for the clinic to approve it.";

                InsertLog("Update Appointment", user.Id, SD.AuditUpdate);

                return RedirectToAction("Index");
            }

            return View(vm);
        }

        #region API CALLS

        public string GetAllAppointments()
        {
            Patient? user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return String.Empty;

            IEnumerable<Appointment>? list = _unitOfWork.Appointment.GetAll(x => x.PatientId == user.Id, includeProperties: "AppointmentTime,Clinic,AppointmentStatus");
            List<AppointmentListVM> appointments = new List<AppointmentListVM>();

            foreach (var item in list)
            {
                appointments.Add(new AppointmentListVM()
                {
                    Id = item.Id,
                    AppointmentDate = item.AppointmentDate,
                    AppointmentTime = item.AppointmentTime,
                    Clinic = AesOperation.DecryptString(item.Clinic.Name),
                    AppointmentStatus = item.AppointmentStatus.Status
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(appointments, jsonSettings);
        }

        [HttpPost]
        public JsonResult Cancel(int id)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return Json(new { success = false });

            var appointment = _unitOfWork.Appointment.GetFirstOrDefault(x => x.Id == id);

            if (appointment == null)
                return Json(new { success = false });

            appointment.AppointmentStatusId = 3;
            appointment.LastModified = DateTime.Now;

            _unitOfWork.Appointment.Update(appointment);
            _unitOfWork.Save();

            ViewData["CurrentPage"] = "appointments";
            TempData["AppointmentSuccess"] = "Cancellation of your appointment was successful.";

            InsertLog("Cancel Appointment", user.Id, SD.AuditUpdate);

            return Json(new { success = true });
        }

        #endregion

        #region HELPER FUNCTIONS

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

        #endregion
    }
}
