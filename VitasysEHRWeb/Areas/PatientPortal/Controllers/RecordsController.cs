using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientPortal.Controllers
{
    [Area("PatientPortal")]
    public class RecordsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PrescriptionVM PrescriptionVM { get; set; }
        public RecordsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ViewData["CurrentPage"] = "records";
            return View();
        }

        public IActionResult MedicalHistory()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            MedicalHistoryVM medicalHistory = new MedicalHistoryVM();

            medicalHistory.MedicalHistory = _unitOfWork.MedicalHistory.GetFirstOrDefault(x => x.PatientId == user.Id);
            if (medicalHistory.MedicalHistory != null)
            {
                medicalHistory.Allergy = _unitOfWork.Allergy.GetFirstOrDefault(x => x.Id == medicalHistory.MedicalHistory.AllergyId);
                medicalHistory.ReviewOfSystem = _unitOfWork.ReviewOfSystem.GetFirstOrDefault(x => x.Id == medicalHistory.MedicalHistory.ROSId);
            }


            InsertLog("View Medical History", user.Id, SD.AuditView);
            ViewData["CurrentPage"] = "medicalHistory";

            return View(medicalHistory);
        }

        public IActionResult DentalCharts()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewData["CurrentPage"] = "dentalChart";

            InsertLog("View Dental Chart List", user.Id, SD.AuditView);

            return View();
        }

        public IActionResult DentalChart(int id)
        {
            try
            {
                var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
                if (user == null)
                    return RedirectToAction("Login", "Account");

                DentalChartVM vm = new();
                vm.DentalChart = _unitOfWork.DentalChart.GetFirstOrDefault(x => x.Id == id, includeProperties: "OralCavity");

                if (vm.DentalChart.EncounterDate == null)
                    return View("Error");

                DateTime encounterDate = (DateTime)vm.DentalChart.EncounterDate;

                vm.EncounterDate = encounterDate.ToString("MM/dd/yyyy hh:mm:ss tt");
                vm.ToothList = new();

                var toothList = _unitOfWork.ToothDetail.GetAll(x => x.DentalChartId == id);

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

                if (vm.DentalChart == null)
                    return View("Error");

                if (vm.DentalChart.OralCavity == null)
                    return View("Error");

                InsertLog("View Dental Chart", user.Id, SD.AuditView);

                ViewData["CurrentPage"] = "dentalChart";
                return View(vm);
            }
            catch
            {
                return View("Error");
            }
        }

        public IActionResult Documents()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            InsertLog("View Document Folders", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "document";
            return View();
        }

        public IActionResult ViewDocument()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.PatientId == user.Id, includeProperties: "FolderType");
            

            InsertLog("View Documents List", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "document";
            return View(vm);
        }

        public IActionResult ViewImage(int id)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Document = _unitOfWork.Document.GetFirstOrDefault(x => x.Id == id);
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == vm.Document.FolderId);

            
            InsertLog("View Document", user.Id, SD.AuditView);

            return PartialView("_ViewDocument", vm);
        }

        public IActionResult Prescriptions()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            InsertLog("View Prescriptions list", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "prescription";
            return View();
        }

        public IActionResult ViewPrescription(int prescriptionId)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            PrescriptionVM? prescriptionVM = new PrescriptionVM() {
                Prescription = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == prescriptionId, includeProperties: "Clinic,Patient"),
            };

            //Clinic Decryption
            prescriptionVM.ClinicName = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.Name);
            prescriptionVM.Address = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.Address);
            prescriptionVM.City = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.City);
            prescriptionVM.Province = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.Province);
            prescriptionVM.ZipCode = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.ZipCode);
            prescriptionVM.EmailAddress = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.EmailAddress);
            prescriptionVM.MobilePhone = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.MobilePhone);
            prescriptionVM.OfficePhone = AesOperation.DecryptString(prescriptionVM.Prescription.Clinic.OfficePhone);

            //Patient Decryption
            prescriptionVM.Fullname = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.FirstName) + " "+
                AesOperation.DecryptString(prescriptionVM.Prescription.Patient.MiddleName) + " " +
                AesOperation.DecryptString(prescriptionVM.Prescription.Patient.LastName);
            prescriptionVM.Gender = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.Gender);
            prescriptionVM.DOB = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.DOB);
            prescriptionVM.PatientAddress = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.Address);
            prescriptionVM.PatientCity = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.City);
            prescriptionVM.PatientProvince = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.Province);
            prescriptionVM.PatientZipCode = AesOperation.DecryptString(prescriptionVM.Prescription.Patient.ZipCode);

            prescriptionVM.Drug = prescriptionVM.Prescription.Drug;
            prescriptionVM.Dose = prescriptionVM.Prescription.Dose;
            prescriptionVM.Dosage = prescriptionVM.Prescription.Dosage;
            prescriptionVM.Sig = prescriptionVM.Prescription.Sig;
            prescriptionVM.Quantity = prescriptionVM.Prescription.Quantity;
            prescriptionVM.DateAdded = prescriptionVM.Prescription.DateAdded.ToString("MM/dd/yyyy");

            var cultureInfo = new CultureInfo("en-PH");
            DateTime Dob = DateTime.ParseExact(prescriptionVM.DOB, "MM/dd/yyyy", null);
            
            var currentYear = DateTime.Now.Year;

            prescriptionVM.Age = currentYear - Dob.Year; 


            InsertLog("View Prescription", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "prescription";
            return View(prescriptionVM);
        }

        public IActionResult Treatments()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            InsertLog("View Treatments list", user.Id, SD.AuditView);
            ViewData["CurrentPage"] = "treatmentRecord";
            return View();
        }


        #region API CALLS
        [HttpGet]
        public string GetAllFolders()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            DocumentsFolderVM folder = new DocumentsFolderVM();
            folder.FolderList = _unitOfWork.Folder.GetAll(x => x.PatientId == user.Id, includeProperties: "FolderType,Clinic");
            List<DocumentsFolderVM> folders = new List<DocumentsFolderVM>();

            foreach(var obj in folder.FolderList)
            {
                folders.Add(new DocumentsFolderVM
                {
                    Id = obj.Id,
                    Name = obj.Name,
                    ClinicName = AesOperation.DecryptString(obj.Clinic.Name),
                    DateAdded = obj.DateAdded,
                    LastModified = obj.LastModified,
                    Type = obj.FolderType.Type

                });
            }

            folder.Patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == user.Id);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(folders, jsonSettings);
        }
        [HttpGet]
        public string GetAllPrescriptions()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            var prescriptionFromDb = _unitOfWork.Prescription.GetAll(x => x.PatientId == user.Id, includeProperties:"Clinic");
            List<PrescriptionVM> prescriptions = new List<PrescriptionVM>();

            foreach (var obj in prescriptionFromDb)
            {
                prescriptions.Add(new PrescriptionVM
                {
                    Id = obj.Id,
                    DateAdded = obj.DateAdded.ToString(),
                    ClinicName = AesOperation.DecryptString(obj.Clinic.Name),
                    Drug = obj.Drug,
                    Dosage = obj.Dosage,
                    Dose = obj.Dose,
                    Quantity = obj.Quantity,
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(prescriptions, jsonSettings);
        }
        [HttpGet]
        public string GetAllDocuments(int id)
        {
            DocumentsFolderVM vm = new DocumentsFolderVM();
            vm.Folder = _unitOfWork.Folder.GetFirstOrDefault(x => x.Id == id, includeProperties: "FolderType");
            vm.DocumentList = _unitOfWork.Document.GetAll().Where(x => x.FolderId == vm.Folder.Id && x.IsShared == true);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(vm.DocumentList, jsonSettings);
        }
        [HttpGet]
        public string GetAllTreatments()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            var treatmentFromDb = _unitOfWork.TreatmentRecord.GetAll(x => x.PatientId == user.Id && x.InvoiceId != null, includeProperties: "Treatment,Clinic,Invoice");
            List<TreatmentRecordVM> treatments = new List<TreatmentRecordVM>();
            

            foreach(var obj in treatmentFromDb)
            {
                treatments.Add(new TreatmentRecordVM
                {
                    InvoiceId = obj.InvoiceId,
                    TreatmentName = obj.Treatment.Name,
                    ToothNumbers = obj.ToothNumbers,
                    ClinicName = AesOperation.DecryptString(obj.Clinic.Name),
                    Dentists = obj.Dentists,
                    Quantity = obj.Quantity,
                    Price = obj.TotalPrice,
                    DateCreated = obj.DateCreated.ToString(),
                    LastModified = obj.LastModified.ToString()
                });
            }
          
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(treatments, jsonSettings);
        }

        [HttpGet]
        public string GetDentalCharts()
        {
            var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (patient == null) return String.Empty;

            var charts = _unitOfWork.DentalChart.GetAll(x => x.PatientId == patient.Id && x.IsArchived == false && x.Status == "Completed", includeProperties: "Clinic");

            if (charts == null) return String.Empty;

            var decryptedCharts = new List<DentalChartListVM>();

            foreach (var obj in charts)
            {
                if (obj.Clinic == null)
                    continue;

                decryptedCharts.Add(new DentalChartListVM
                {
                    Id = obj.Id,
                    Clinic = AesOperation.DecryptString(obj.Clinic.Name),
                    EncounterDate = obj.EncounterDate,
                    LastModified = obj.LastModified
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedCharts, jsonSettings);
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
