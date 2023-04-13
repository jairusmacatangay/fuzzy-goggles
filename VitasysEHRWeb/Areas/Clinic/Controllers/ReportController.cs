using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;
using Microsoft.AspNetCore.Authorization;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner)]
    public class ReportController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportController(IUnitOfWork unitOfWork)
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

            ReportVM vm = new ReportVM();
            vm.ReportList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Patients", Value = "1" },
                new SelectListItem { Text = "Treatments Performed", Value = "2" },
                new SelectListItem { Text = "Invoices", Value = "3" },
                new SelectListItem { Text = "Audit Logs", Value = "4" },
                new SelectListItem { Text = "Dental Summary", Value = "5" },
            };

            
            InsertLog("View Report Generation", user.Id, user.ClinicId, SD.AuditView);

            return View(vm);
        }

        #region API CALLS

        public IActionResult LoadPatientReport()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Patient Report", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_PatientReport");
        }

        public string GetAllPatients()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();

            var patients = _unitOfWork.ClinicPatient.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "Patient");

            if (patients == null) return String.Empty;

            List<Patient> decryptedPatients = new List<Patient>();

            foreach (var obj in patients)
            {
                decryptedPatients.Add(new Patient
                {
                    Id = obj.Patient.Id,
                    FirstName = AesOperation.DecryptString(obj.Patient.FirstName),
                    MiddleName = AesOperation.DecryptString(obj.Patient.MiddleName),
                    LastName = AesOperation.DecryptString(obj.Patient.LastName),
                    DOB = AesOperation.DecryptString(obj.Patient.DOB),
                    Gender = AesOperation.DecryptString(obj.Patient.Gender),
                    Address = AesOperation.DecryptString(obj.Patient.Address),
                    City = AesOperation.DecryptString(obj.Patient.City),
                    Province = AesOperation.DecryptString(obj.Patient.Province),
                    ZipCode = AesOperation.DecryptString(obj.Patient.ZipCode),
                    HomeNumber = AesOperation.DecryptString(obj.Patient.HomeNumber),
                    MobileNumber = AesOperation.DecryptString(obj.Patient.MobileNumber),
                    ProfPicUrl = AesOperation.DecryptString(obj.Patient.ProfPicUrl),
                    DateAdded = obj.Patient.DateAdded,
                    LastModified = obj.Patient.LastModified,
                    Email = obj.Patient.Email,
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedPatients, jsonSettings);
        }

        public IActionResult LoadInvoiceReport()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Invoice Report", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_InvoiceReport");
        }

        public string GetAllInvoices()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();

            var invoices = _unitOfWork.Invoice.GetAll(x => 
                x.ClinicId == user.ClinicId && 
                x.IsArchived == false && 
                x.InvoiceStatus != "Draft", 
                includeProperties: "PaymentStatus,Patient");

            if(invoices == null) return String.Empty;

            List<InvoiceListVM> decryptedInvoices = new List<InvoiceListVM>();

            foreach (var obj in invoices)
            {
                decryptedInvoices.Add(new InvoiceListVM
                {
                    PatientName = AesOperation.DecryptString(obj.Patient.LastName) + ", " + AesOperation.DecryptString(obj.Patient.FirstName) + " " + AesOperation.DecryptString(obj.Patient.MiddleName),
                    InvoiceDate = obj.InvoiceDate,
                    InvoiceStatus = obj.InvoiceStatus,
                    PaymentStatus = obj.PaymentStatus.Status,
                    TotalAmount = obj.TotalAmount
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedInvoices, jsonSettings);
        }

        public IActionResult LoadAuditLogsReport()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Audit Logs Report", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_AuditLogsReport");
        }

        public string GetAllAuditLogs()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();

            var auditLogs = _unitOfWork.AuditLog.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "ApplicationUser");

            if (auditLogs == null) return String.Empty;

            List<AuditLog> decryptedAuditLogs = new List<AuditLog>();

            foreach (var obj in auditLogs)
            {
                decryptedAuditLogs.Add(new AuditLog
                {
                    Id = obj.Id,
                    ApplicationUser = new ApplicationUser()
                    {
                        FirstName = AesOperation.DecryptString(obj.ApplicationUser.FirstName),
                        MiddleName = AesOperation.DecryptString(obj.ApplicationUser.MiddleName),
                        LastName = AesOperation.DecryptString(obj.ApplicationUser.LastName),
                    },
                    DateAdded = obj.DateAdded,
                    ActivityType = obj.ActivityType,
                    Description = obj.Description,
                    Device = obj.Device
                });
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedAuditLogs, jsonSettings);
        }

        public IActionResult LoadTreatmentReport()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Audit Logs Report", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_TreatmentRecordsReport");
        }

        public string GetAllTreatmentRecords()
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var user = GetCurrentUser();

            var treatmentRecords = _unitOfWork.TreatmentRecord.GetAll(x => x.ClinicId == user.ClinicId && x.InvoiceId != null, includeProperties:"Treatment,Clinic");

            if (treatmentRecords == null) return String.Empty;

            List<TreatmentRecord> decryptedTreatmentRecords = new List<TreatmentRecord>();

            foreach (var obj in treatmentRecords)
            {
                decryptedTreatmentRecords.Add(new TreatmentRecord
                {
                    InvoiceId = obj.InvoiceId,
                    Treatment = obj.Treatment,
                    ToothNumbers = obj.ToothNumbers,
                    Clinic = new VitasysEHR.Models.Clinic()
                    {
                        Name = AesOperation.DecryptString(obj.Clinic.Name)
                    },
                    Dentists = obj.Dentists,
                    Quantity = obj.Quantity,
                    TotalPrice = obj.TotalPrice,
                    DateCreated = obj.DateCreated,
                    LastModified = obj.LastModified
                    
                });;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedTreatmentRecords, jsonSettings);
        }

        public IActionResult LoadDentalSummaryForm()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Dental Summary Report", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_DentalSummaryForm");
        }

        public IActionResult GetPatients(string term)
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();

            var patients = _unitOfWork.ClinicPatient.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "Patient");

            if (patients == null) return View("Error");

            List<AutocompletePatientVM> decryptedPatients = new List<AutocompletePatientVM>();

            foreach (var obj in patients)
            {
                if (obj.Patient == null) continue;

                decryptedPatients.Add(new AutocompletePatientVM
                {
                    Id = obj.Patient.Id,
                    Name = AesOperation.DecryptString(obj.Patient.LastName) + ", " + AesOperation.DecryptString(obj.Patient.FirstName) + " " + AesOperation.DecryptString(obj.Patient.MiddleName),
                });
            }

            return Json(decryptedPatients.Where(x => x.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)));
        }

        [AllowAnonymous]
        public IActionResult LoadDentalSummaryReport(int patientId, bool isPrint, int clinicId)
        {
            try
            {
                if (clinicId == 0)
                {
                    var user = GetCurrentUser();

                    if (user == null) return View("Error");

                    if (user.ClinicId == null) return View("Error");

                    InsertLog("View Dental Summary", user.Id, user.ClinicId, SD.AuditView);

                    clinicId = (int)user.ClinicId;
                }
                
                DentalChartVM vm = new();
                var chartList = _unitOfWork.DentalChart
                    .GetAll(x => x.PatientId == patientId &&
                    x.ClinidId == clinicId &&
                    x.Status == "Completed" &&
                    x.IsArchived == false,
                    includeProperties: "OralCavity")
                    .ToList();

                if (chartList.Count == 0)
                    return PartialView("_NoDentalChart");

                vm.DentalChart = chartList.OrderByDescending(x => x.EncounterDate).First();

                if (vm.DentalChart.OralCavity == null)
                    return PartialView("Error");

                if (vm.DentalChart.EncounterDate == null)
                    return PartialView("Error");



                DateTime encounterDate = (DateTime)vm.DentalChart.EncounterDate;

                vm.EncounterDate = encounterDate.ToString("MM/dd/yyyy hh:mm:ss tt");
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

                if (isPrint == true)
                    return View("DentalSummary", vm);

                return PartialView("_DentalSummary", vm);
            }
            catch
            {
                return PartialView("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintDentalSummary(int patientId)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            await using var page = await browser.NewPageAsync();

            var user = GetCurrentUser();

            if (user == null) return View("Error");

            string? url = Url.Action("LoadDentalSummaryReport", "Report", new { area = "Clinic", patientId = patientId, isPrint = true, clinicId = user.ClinicId });

            await page.GoToAsync($"{Request.Scheme}://{Request.Host}{url}");

            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                Landscape = true,
            });

            InsertLog("Print Dental Summary", user.Id, user.ClinicId, "Generated a record as a pdf file.");

            return File(pdfContent, "application/pdf", $"DentalSummary-{patientId}.pdf");
        }

        #endregion

        public string? GetAllTreatmentsPerformed(int? id)
        {
            if (AuthorizeAccess() == false) return String.Empty;
            /*var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;*/

            //get current user
            var currentUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == "e3a1427f-3b27-4a9a-ad63-df32b43886e3");
            //get the clinic of the current user
            var clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == currentUser.ClinicId);
            //get the patients of that clinic
            var clinicPatients = _unitOfWork.ClinicPatient.GetAll(includeProperties: "Patient").Where(x => x.ClinicId == clinic.Id);

            if(clinicPatients.Any(x => x.PatientId == id))
            {
                //get the patient
                var clinicPatient = clinicPatients.Where(x => x.PatientId == id).FirstOrDefault();
                var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == clinicPatient.PatientId);
                //get the treatment record of the patient
                var treatmentsPerformed = _unitOfWork.TreatmentRecord.GetAll(includeProperties: "Treatment,Treatment.TreatmentType").Where(x => x.PatientId == patient.Id);
                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
                jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                return JsonConvert.SerializeObject(treatmentsPerformed, jsonSettings);
            }
            else
            {
                return null;
            }
            
        }

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
