using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int id)
        {
            try
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
                Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
                var check = _unitOfWork.ClinicPatient.GetFirstOrDefault(x => x.ClinicId == user.ClinicId && x.PatientId == patient.Id);

                if (check == null)
                {
                    TempData["error"] = "Patient id could not be found";
                    return View();
                }

                if (patient != null)
                {
                    string name = $"{AesOperation.DecryptString(patient.FirstName)} {AesOperation.DecryptString(patient.LastName)}";

                    HttpContext.Session.SetString(SD.SessionKeyPatientName, name);
                    HttpContext.Session.SetInt32(SD.SessionKeyPatientId, patient.Id);

                    if (!string.IsNullOrEmpty(patient.ProfPicUrl))
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, AesOperation.DecryptString(patient.ProfPicUrl));
                    else
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, "/img/patients/prof-pic-placeholder.png");
                }
                else
                {
                    return NotFound();
                }

                DashboardVM vm = new();
                vm.PatientId = id;
                vm.DentalChartVM = GetDentalChart(id, user.ClinicId);
                vm.MedicalHistoryVM = GetMedicalHistory(id);
                vm.TreatmentRecords = GetTreatmentRecords(id);
                vm.Prescriptions = GetPrescriptions(id);
                vm.Folders = GetFolders(id, user.ClinicId);
                vm.Documents = GetDocuments(id, user.ClinicId);
                vm.Reminders = GetReminders(id, user.ClinicId);

                InsertLog("View Dashboard", user.Id, user.ClinicId, SD.AuditView);

                ViewData["CurrentPage"] = "dashboard";
                return View(vm);
            }
            catch (InvalidOperationException ex)
            {
                Console.Write(ex.Message);
                return View("Error");
            }
        }

        #region HELPER FUNCTIONS
        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string? userid = claim.Value;
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userid);
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

        public DentalChartVM GetDentalChart(int patientId, int? clinicId)
        {
            DentalChartVM vm = new();
            DentalChartVM empty = new();

            var chartList = _unitOfWork.DentalChart
                .GetAll(x => x.PatientId == patientId &&
                x.ClinidId == clinicId &&
                x.Status == "Completed" &&
                x.IsArchived == false,
                includeProperties: "OralCavity")
                .ToList();

            if (chartList.Count == 0) return empty;

            vm.DentalChart = chartList.OrderByDescending(x => x.EncounterDate).First();

            if (vm.DentalChart.OralCavity == null) return empty;

            if (vm.DentalChart.EncounterDate == null) return empty;

            DateTime encounterDate = (DateTime)vm.DentalChart.EncounterDate;

            vm.EncounterDate = encounterDate.ToString("MMMM dd, yyyy hh:mm:ss tt");
            vm.ToothList = new();

            var toothList = _unitOfWork.ToothDetail.GetAll(x => x.DentalChartId == vm.DentalChart.Id);

            foreach (var item in toothList)
            {
                vm.ToothList.Add(new ToothVM
                {
                    Id = item.Id,
                    ToothNo = item.ToothNo,
                    Type = item.Type,
                    Condition = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Condition),
                    BuccalOrLabial = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.BuccalOrLabial),
                    Lingual = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Lingual),
                    Mesial = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Mesial),
                    Distal = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Distal),
                    Occlusal = _unitOfWork.ToothLabel.GetFirstOrDefault(x => x.Id == item.Occlusal)
                });
            }

            vm.Tooth55 = vm.ToothList.Find(x => x.ToothNo == 55);
            vm.Tooth54 = vm.ToothList.Find(x => x.ToothNo == 54);
            vm.Tooth53 = vm.ToothList.Find(x => x.ToothNo == 53);
            vm.Tooth52 = vm.ToothList.Find(x => x.ToothNo == 52);
            vm.Tooth51 = vm.ToothList.Find(x => x.ToothNo == 51);
            vm.Tooth61 = vm.ToothList.Find(x => x.ToothNo == 61);
            vm.Tooth62 = vm.ToothList.Find(x => x.ToothNo == 62);
            vm.Tooth63 = vm.ToothList.Find(x => x.ToothNo == 63);
            vm.Tooth64 = vm.ToothList.Find(x => x.ToothNo == 64);
            vm.Tooth65 = vm.ToothList.Find(x => x.ToothNo == 65);
            vm.Tooth18 = vm.ToothList.Find(x => x.ToothNo == 18);
            vm.Tooth17 = vm.ToothList.Find(x => x.ToothNo == 17);
            vm.Tooth16 = vm.ToothList.Find(x => x.ToothNo == 16);
            vm.Tooth15 = vm.ToothList.Find(x => x.ToothNo == 15);
            vm.Tooth14 = vm.ToothList.Find(x => x.ToothNo == 14);
            vm.Tooth13 = vm.ToothList.Find(x => x.ToothNo == 13);
            vm.Tooth12 = vm.ToothList.Find(x => x.ToothNo == 12);
            vm.Tooth11 = vm.ToothList.Find(x => x.ToothNo == 11);
            vm.Tooth21 = vm.ToothList.Find(x => x.ToothNo == 21);
            vm.Tooth22 = vm.ToothList.Find(x => x.ToothNo == 22);
            vm.Tooth23 = vm.ToothList.Find(x => x.ToothNo == 23);
            vm.Tooth24 = vm.ToothList.Find(x => x.ToothNo == 24);
            vm.Tooth25 = vm.ToothList.Find(x => x.ToothNo == 25);
            vm.Tooth26 = vm.ToothList.Find(x => x.ToothNo == 26);
            vm.Tooth27 = vm.ToothList.Find(x => x.ToothNo == 27);
            vm.Tooth28 = vm.ToothList.Find(x => x.ToothNo == 28);
            vm.Tooth48 = vm.ToothList.Find(x => x.ToothNo == 48);
            vm.Tooth47 = vm.ToothList.Find(x => x.ToothNo == 47);
            vm.Tooth46 = vm.ToothList.Find(x => x.ToothNo == 46);
            vm.Tooth45 = vm.ToothList.Find(x => x.ToothNo == 45);
            vm.Tooth44 = vm.ToothList.Find(x => x.ToothNo == 44);
            vm.Tooth43 = vm.ToothList.Find(x => x.ToothNo == 43);
            vm.Tooth42 = vm.ToothList.Find(x => x.ToothNo == 42);
            vm.Tooth41 = vm.ToothList.Find(x => x.ToothNo == 41);
            vm.Tooth31 = vm.ToothList.Find(x => x.ToothNo == 31);
            vm.Tooth32 = vm.ToothList.Find(x => x.ToothNo == 32);
            vm.Tooth33 = vm.ToothList.Find(x => x.ToothNo == 33);
            vm.Tooth34 = vm.ToothList.Find(x => x.ToothNo == 34);
            vm.Tooth35 = vm.ToothList.Find(x => x.ToothNo == 35);
            vm.Tooth36 = vm.ToothList.Find(x => x.ToothNo == 36);
            vm.Tooth37 = vm.ToothList.Find(x => x.ToothNo == 37);
            vm.Tooth38 = vm.ToothList.Find(x => x.ToothNo == 38);
            vm.Tooth85 = vm.ToothList.Find(x => x.ToothNo == 85);
            vm.Tooth84 = vm.ToothList.Find(x => x.ToothNo == 84);
            vm.Tooth83 = vm.ToothList.Find(x => x.ToothNo == 83);
            vm.Tooth82 = vm.ToothList.Find(x => x.ToothNo == 82);
            vm.Tooth81 = vm.ToothList.Find(x => x.ToothNo == 81);
            vm.Tooth71 = vm.ToothList.Find(x => x.ToothNo == 71);
            vm.Tooth72 = vm.ToothList.Find(x => x.ToothNo == 72);
            vm.Tooth73 = vm.ToothList.Find(x => x.ToothNo == 73);
            vm.Tooth74 = vm.ToothList.Find(x => x.ToothNo == 74);
            vm.Tooth75 = vm.ToothList.Find(x => x.ToothNo == 75);

            return vm;
        }
        
        public MedicalHistoryVM GetMedicalHistory(int patientId)
        {
            MedicalHistoryVM vm = new();
            MedicalHistoryVM empty = new();

            vm.MedicalHistory = _unitOfWork.MedicalHistory.GetFirstOrDefault(x => x.PatientId == patientId);

            if (vm.MedicalHistory == null) return empty;

            vm.Allergy = _unitOfWork.Allergy.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.AllergyId);
            vm.ReviewOfSystem = _unitOfWork.ReviewOfSystem.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.ROSId);

            return vm;
        }

        public List<TreatmentRecord> GetTreatmentRecords(int id)
        {
            List<TreatmentRecord> empty = new();

            var list = _unitOfWork.TreatmentRecord.GetAll(x => 
                x.PatientId == id && 
                x.IsArchived == false && x.Treatment != null, 
                includeProperties: "Treatment")
                .ToList();

            if (list.Count < 0) return empty;
            
            return list;
        }

        public List<Prescription> GetPrescriptions(int id)
        {
            List<Prescription> empty = new();

            List<Prescription> list = _unitOfWork.Prescription.GetAll(x => 
                x.PatientId == id && 
                x.IsArchived == false)
                .ToList();

            if (list.Count < 0) return empty;

            return list;
        }

        public List<Folder> GetFolders(int patientId, int? clinicId)
        {
            List<Folder> empty = new();

            var list = _unitOfWork.Folder.GetAll(x => 
                x.PatientId == patientId &&
                x.ClinicId == clinicId,
                includeProperties: "FolderType")
                .ToList();

            if (list.Count < 0) return empty;

            return list;
        }

        public List<Document> GetDocuments(int patientId, int? clinicId)
        {
            List<Document> documents = new();
            List<Document> empty = new();

            var folders = GetFolders(patientId, clinicId);
            
            if (folders.Count < 0) return empty;

            foreach (var folder in folders)
            {
                var files = _unitOfWork.Document.GetAll(x => 
                    x.FolderId == folder.Id && 
                    x.IsArchived == false,
                    includeProperties: "Folder")
                    .ToList();

                if (files.Count < 0) continue;

                foreach (var file in files)
                {
                    documents.Add(file);
                }
            }

            if (documents.Count < 0) return empty;

            return documents.OrderByDescending(x => x.DateAdded).ToList();
        }

        public List<Reminder> GetReminders(int patientId, int? clinicId)
        {
            List<Reminder> empty = new();

            var reminders = _unitOfWork.Reminder.GetAll(x => 
                x.PatientId == patientId && 
                x.ClinicId == clinicId)
                .ToList();

            if (reminders.Count < 0) return empty;

            return reminders;
        }

        #endregion
    }
}