using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Globalization;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist)]
    public class PrescriptionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ViewData["CurrentPage"] = "prescription";
            ApplicationUser user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }
            InsertLog("View Prescription List", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PrescriptionFormVM obj)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = GetCurrentUser();

                if (user.ClinicId == null) return View("Error");

                Prescription prescription = new();
                prescription.ClinicId = (int)user.ClinicId;
                prescription.PatientId = obj.PatientId;
                prescription.Drug = obj.Drug;
                prescription.Dose = obj.Dose;
                prescription.Dosage = obj.Dosage;
                prescription.Sig = obj.Sig;
                prescription.UserId = user.Id;
                prescription.Quantity = obj.Quantity;
                prescription.OtherNotes = obj.OtherNotes;

                CultureInfo cultureInfo = new("en-PH");

                if (obj.StartDate != null)
                {
                    prescription.StartDate = DateTime.Parse(obj.StartDate, cultureInfo);
                }

                if (obj.EndDate != null)
                {
                    prescription.EndDate = DateTime.Parse(obj.EndDate, cultureInfo);
                }
                
                _unitOfWork.Prescription.Add(prescription);
                _unitOfWork.Save();
                                
                InsertLog("Create Prescription", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Prescription created successfully";

                return RedirectToAction(nameof(Index));
            }
            return View("Index", obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(PrescriptionFormVM obj)
        {
            if (ModelState.IsValid)
            {
                Prescription prescription = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == obj.Id);

                if (prescription == null) return View("Error");

                prescription.Drug = obj.Drug;
                prescription.Dose = obj.Dose;
                prescription.Dosage = obj.Dosage;
                prescription.Sig = obj.Sig;
                prescription.Quantity = obj.Quantity;
                prescription.OtherNotes = obj.OtherNotes;
                prescription.LastModified = DateTime.Now;

                CultureInfo cultureInfo = new("en-PH");

                if (obj.StartDate != null)
                {
                    prescription.StartDate = DateTime.Parse(obj.StartDate,  cultureInfo);
                }

                if (obj.EndDate != null)
                {
                    prescription.EndDate = DateTime.Parse(obj.EndDate, cultureInfo);
                }

                _unitOfWork.Prescription.Update(prescription);
                _unitOfWork.Save();

                ApplicationUser user = GetCurrentUser();
                InsertLog("Update Prescription", user.Id, user.ClinicId, SD.AuditUpdate);

                TempData["success"] = "Prescription updated successfully";

                return RedirectToAction(nameof(Index));
            }
            return View(nameof(Index));
        }

        #region API Calls
        [HttpGet]
        public string GetPrescriptions(int id, string status)
        {
            IEnumerable<Prescription> prescriptions = _unitOfWork.Prescription.GetAll(x => x.PatientId == id);

            switch (status)
            {
                case "archived":
                    prescriptions = prescriptions.Where(x => x.IsArchived == true);
                    break;
                case "active":
                    prescriptions = prescriptions.Where(x => x.IsArchived == false);
                    break;
                default:
                    break;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(prescriptions, jsonSettings);
        }

        [HttpGet]
        public IActionResult GetPrescription(int id)
        {
            PrescriptionVM vm = new();

            vm.Prescription = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == id);

            if (vm.Prescription == null) return View("Error");

            if (vm.Prescription.StartDate != null)
            {
                DateTime startDate = (DateTime)vm.Prescription.StartDate;
                vm.StartDate = startDate.ToString("dddd, MMMM dd, yyyy");
            }

            if (vm.Prescription.EndDate != null)
            {
                DateTime endDate = (DateTime)vm.Prescription.EndDate;
                vm.EndDate = endDate.ToString("dddd, MMMM dd, yyyy");
            }

            ApplicationUser presciber = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == vm.Prescription.UserId);

            vm.FirstName = AesOperation.DecryptString(presciber.FirstName);
            vm.LastName = AesOperation.DecryptString(presciber.LastName);

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Prescription", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewPrescription", vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetPrint(int id, bool isPrint, int clinicId)
        {
            PrescriptionVM? vm = new PrescriptionVM();
            vm.Prescription = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == id, includeProperties: "Clinic,Patient");

            if (vm.Prescription == null) return View("Error");

            if (vm.Prescription.Patient == null) return View("Error");

            //Clinic Decryption
            vm.ClinicName = AesOperation.DecryptString(vm.Prescription.Clinic.Name);
            vm.Address = AesOperation.DecryptString(vm.Prescription.Clinic.Address);
            vm.City = AesOperation.DecryptString(vm.Prescription.Clinic.City);
            vm.Province = AesOperation.DecryptString(vm.Prescription.Clinic.Province);
            vm.ZipCode = AesOperation.DecryptString(vm.Prescription.Clinic.ZipCode);
            vm.EmailAddress = AesOperation.DecryptString(vm.Prescription.Clinic.EmailAddress);
            vm.MobilePhone = AesOperation.DecryptString(vm.Prescription.Clinic.MobilePhone);
            vm.OfficePhone = AesOperation.DecryptString(vm.Prescription.Clinic.OfficePhone);

            //Patient Decryption
            vm.Fullname = AesOperation.DecryptString(vm.Prescription.Patient.FirstName) + " " +
                AesOperation.DecryptString(vm.Prescription.Patient.MiddleName) + " " +
                AesOperation.DecryptString(vm.Prescription.Patient.LastName);
            vm.Gender = AesOperation.DecryptString(vm.Prescription.Patient.Gender);
            vm.DOB = AesOperation.DecryptString(vm.Prescription.Patient.DOB);

            vm.Drug = vm.Prescription.Drug;
            vm.Dose = vm.Prescription.Dose;
            vm.Dosage = vm.Prescription.Dosage;
            vm.Sig = vm.Prescription.Sig;
            vm.Quantity = vm.Prescription.Quantity;
            vm.DateAdded = vm.Prescription.DateAdded.ToString("MM/dd/yyyy");

            CultureInfo cultureInfo = new("en-PH");
            DateTime Dob = Convert.ToDateTime(DateTime.ParseExact(vm.DOB, "MM/dd/yyyy", cultureInfo));
            var currentYear = DateTime.Now.Year;

            vm.Age = currentYear - Dob.Year;

            if (clinicId == 0)
            {
                ApplicationUser user = GetCurrentUser();
                InsertLog("View Prescription", user.Id, user.ClinicId, SD.AuditView);
            }

            if (isPrint == true)
                return View("PrescriptionTemplate", vm);
            
            return PartialView("_ViewPrint", vm);
        }

        [HttpGet]
        public async Task<IActionResult> PrintPrescription(int prescriptionId)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            await using var page = await browser.NewPageAsync();

            var user = GetCurrentUser();

            if (user == null) return View("Error");

            string? url = Url.Action("GetPrint", "Prescription", new { area = "PatientRecords", id = prescriptionId, isPrint = true, clinicId = user.ClinicId });

            await page.GoToAsync($"{Request.Scheme}://{Request.Host}{url}");

            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A5,
                PrintBackground = true,
            });

            InsertLog("Print Dental Summary", user.Id, user.ClinicId, "Generated a record as a pdf file.");

            return File(pdfContent, "application/pdf", $"Prescription-{prescriptionId}.pdf");
        }

        [HttpGet]
        public IActionResult LoadAddForm()
        {
            ApplicationUser user = GetCurrentUser();
            InsertLog("View Add Prescription Form", user.Id, user.ClinicId, SD.AuditCreate);
            return PartialView("_AddPrescriptionForm");
        }

        [HttpGet]
        public IActionResult LoadEditForm(int id)
        {
            ApplicationUser user = GetCurrentUser();
            InsertLog("View Edit Prescription Form", user.Id, user.ClinicId, SD.AuditUpdate);

            Prescription prescription = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == id);
            
            if (prescription == null) return NotFound();

            PrescriptionFormVM vm = new();
            vm.Id = prescription.Id;
            vm.Drug = prescription.Drug;
            vm.Dose = prescription.Dose;
            vm.Dosage = prescription.Dosage;
            vm.Sig = prescription.Sig;
            vm.Quantity = prescription.Quantity;
            vm.OtherNotes = prescription.OtherNotes;

            if (prescription.StartDate != null)
            {
                DateTime startDate = (DateTime)prescription.StartDate;
                vm.StartDate = startDate.ToString("yyyy-MM-dd");
            }
            
            if (prescription.EndDate != null)
            {
                DateTime endDate = (DateTime)prescription.EndDate;
                vm.EndDate = endDate.ToString("yyyy-MM-dd");
            }
            
            return PartialView("_EditPrescriptionForm", vm);
        }

        [HttpPost]
        public IActionResult Archive(int? id)
        {
            Prescription obj = _unitOfWork.Prescription.GetFirstOrDefault(x => x.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Prescription does not exist." });
            }
            obj.IsArchived = true;
            _unitOfWork.Prescription.Update(obj);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Archive Prescription", user.Id, user.ClinicId, SD.AuditArchive);

            return Json(new { success = true, message = "Archived successfully!" });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Prescription.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Prescription does not exist." });
            }
            _unitOfWork.Prescription.Remove(obj);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Delete Prescription", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Delete Successful!" });
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