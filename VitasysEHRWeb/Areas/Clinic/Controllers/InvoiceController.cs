using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
    public class InvoiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvoiceController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the View Invoice List page
        /// </summary>
        /// <param name="status">Used to filter the records by status</param>
        /// <returns>returns view for Index</returns>
        public IActionResult Index(string status)
        {
            var user = GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }
            if (AuthorizeAccess() == false) return View("Error");

            if (status != null)
                ViewData["filter"] = status;

            
            InsertLog("View Invoice List", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        /// <summary>
        /// Displays the Create Invoice Draft page
        /// </summary>
        /// <param name="invoiceId">Id of Invoice</param>
        /// <returns>returns the Create view with ViewModel</returns>
        [HttpGet]
        public IActionResult Create(int invoiceId)
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();

            //Get list of treatment record
            List<TreatmentRecord> treatmentList = _unitOfWork.TreatmentRecord.GetAll(x => x.InvoiceId == invoiceId).ToList();

            List<TreatmentRecordListVM> vmList = new List<TreatmentRecordListVM>();

            //Calculate Total Amount
            decimal totalAmount = 0;
            foreach (var item in treatmentList)
            {
                totalAmount += item.TotalPrice;

                string? treatmentName = null;

                if (item.TreatmentId != null)
                {
                    treatmentName = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == item.TreatmentId).Name;
                }

                vmList.Add(new TreatmentRecordListVM()
                {
                    TreatmentRecordId = item.Id,
                    TreatmentName = treatmentName ?? "N/A",
                    InvoiceID = item.InvoiceId,
                    ToothNumbers = item.ToothNumbers ?? "N/A",
                    Dentists = item.Dentists ?? "N/A",
                    TotalPrice = item.TotalPrice,
                    Quantity = item.Quantity
                });
            }

            int patientId = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == invoiceId).PatientId;
            var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == patientId);

            CreateInvoiceVM vm = new CreateInvoiceVM()
            {
                InvoiceId = invoiceId,
                TotalAmount = totalAmount,
                TreatmentRecordList = vmList,
                UntaggedTreatmentRecordList = _unitOfWork.TreatmentRecord.GetAll(x => x.InvoiceId == null && x.Treatment.IsArchived == false && x.Treatment.ClinicId == user.ClinicId, includeProperties: "Treatment").ToList(),
                PatientName = AesOperation.DecryptString(patient.LastName) + ", " + AesOperation.DecryptString(patient.FirstName) + " " + AesOperation.DecryptString(patient.MiddleName)
            };

            InsertLog("View Create Invoice", user.Id, user.ClinicId, SD.AuditView);

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddUntaggedTreatmentRecord(int treatmentId, int invoiceId)
        {
            TreatmentRecord? treatmentRecord = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == treatmentId);

            if (treatmentRecord == null) return BadRequest();

            treatmentRecord.InvoiceId = invoiceId;
            treatmentRecord.LastModified = DateTime.Now;

            _unitOfWork.TreatmentRecord.Update(treatmentRecord);
            _unitOfWork.Save();

            TempData["success"] = "Successfully added untagged treatment record to invoice!";

            ApplicationUser user = GetCurrentUser();
            InsertLog("Added Untagged Treatment Record To Invoice", user.Id, user.ClinicId, SD.AuditUpdate);

            return RedirectToAction("Create", new { invoiceId = invoiceId});
        }

        /// <summary>
        /// This method creates the finalized version of the invoice.
        /// </summary>
        /// <param name="invoiceId">Id of Invoice</param>
        /// <param name="totalAmount">
        /// The total sum of all treatment prices in the invoice
        /// </param>
        /// <returns>
        /// Returns a RedirectToAction to Index after the invoice creation
        /// </returns>
        [HttpPost]
        public IActionResult Create(int invoiceId, decimal totalAmount)
        {
            if (totalAmount <= 0)
            {
                TempData["error"] = "You can't create an invoice if you don't have any treatment records.";
                return RedirectToAction("Create", new { invoiceId = invoiceId });
            }

            var invoice = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == invoiceId);
            invoice.InvoiceDate = DateTime.Now;
            invoice.InvoiceStatus = "Created";

            // Not yet tested
            invoice.TotalAmount = totalAmount;
            invoice.PaymentStatusId = 2;

            _unitOfWork.Invoice.Update(invoice);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Create Invoice", user.Id, user.ClinicId, SD.AuditCreate);

            TempData["success"] = "Successfully created invoice.";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// This creates an invoice with a status of draft. 
        /// </summary>
        /// <param name="patientId">Id of patient</param>
        /// <returns>
        /// returns a json with a callback url that goes to Invoice/Create which will
        /// be executed in the front-end using javascript
        /// </returns>
        [HttpPost]
        public IActionResult CreateInitialInvoice(string patientId)
        {
            Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == Int32.Parse(patientId));

            ApplicationUser user = GetCurrentUser();

            Invoice invoice = new Invoice()
            {
                UserId = user.Id,
                PatientId = patient.Id,
                ClinicId = (int)user.ClinicId,
                PaymentStatusId = 2,
                InvoiceStatus = "Draft"
            };

            _unitOfWork.Invoice.Add(invoice);
            _unitOfWork.Save();

            string? url = Url.Action("Create", "Invoice", new { invoiceId = invoice.Id });
            TempData["success"] = "Successfully created invoice draft!";

            InsertLog("Create Invoice Draft", user.Id, user.ClinicId, SD.AuditCreate);

            return Json(new { url = url });
        }

        /// <summary>
        /// Displays the Add Treatment Record To Invoice page
        /// </summary>
        /// <param name="id">Id of the Treatment Record</param>
        /// <returns>
        /// Returns the View for UpsertTreatment.cshtml together with a ViewModel
        /// </returns>
        [HttpGet]
        public IActionResult UpsertTreatment(int? id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            CreateTreatmentVM vm = new CreateTreatmentVM();
            vm.TreatmentRecord = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);

            if (vm.TreatmentRecord.TreatmentId == null)
                return View(vm);

            vm.Treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == vm.TreatmentRecord.TreatmentId, includeProperties: "TreatmentType");

            var user = GetCurrentUser();
            InsertLog("View UpsertTreatment", user.Id, user.ClinicId, SD.AuditView);

            return View(vm);
        }

        /// <summary>
        /// Creates an empty Treatment Record after clicking on the 
        /// Add Treatment button
        /// </summary>
        /// <param name="invoiceId">Id of invoice</param>
        /// <returns>
        /// Returns a RedirectToAction to UpsertTreatment after
        /// creating empty Treatment Record
        /// </returns>
        [HttpPost]
        public IActionResult CreateTreatment(int invoiceId)
        {
            Invoice invoice = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == invoiceId);

            TreatmentRecord record = new TreatmentRecord()
            {
                PatientId = invoice.PatientId,
                InvoiceId = invoice.Id,
                ClinicId = invoice.ClinicId
            };

            _unitOfWork.TreatmentRecord.Add(record);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Create Treatment", user.Id, user.ClinicId, SD.AuditCreate);

            return RedirectToAction("UpsertTreatment", new { id = record.Id });
        }

        /// <summary>
        /// Adds the treatment, its price and quantity to the TreatmentRecord
        /// </summary>
        /// <param name="treatmentRecordId">Id of the Treatment Record</param>
        /// <param name="treatmentId">Id of Treatment</param>
        [HttpPost]
        public void AddTreatment(int treatmentRecordId ,int treatmentId)
        {
            var user = GetCurrentUser();
            Treatment treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == treatmentId);
            
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == treatmentRecordId);
            record.TreatmentId = treatmentId;
            record.TotalPrice = treatment.Price;
            record.Quantity = 1;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            
            InsertLog("Added Treatment to Invoice", user.Id, user.ClinicId, SD.AuditCreate);
        }

        /// <summary>
        /// Deletes treatment record
        /// </summary>
        /// <param name="id">Id of treament record</param>
        /// <returns>
        /// Returns a json which will be used by sweetalert2 js in the front-end
        /// </returns>
        [HttpDelete]
        public IActionResult DeleteTreatment(int id)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);

            if (record == null)
                return Json(new { success = false, message = "Treatment Record does not exist." });

            _unitOfWork.TreatmentRecord.Remove(record);
            _unitOfWork.Save();

            TempData["success"] = "Successfully deleted treatment record.";

            var user = GetCurrentUser();
            InsertLog("Delete Treatment from Invoice", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true });
        }

        
        [HttpPost]
        public IActionResult RemoveTreatment(int id)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);

            if (record == null)
                return Json(new { success = false, message = "Treatment Record does not exist." });

            record.InvoiceId = null;
            record.LastModified = DateTime.Now;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            TempData["success"] = "Successfully removed treatment record from invoice.";

            var user = GetCurrentUser();
            InsertLog("Removed Treatment from Invoice", user.Id, user.ClinicId, SD.AuditUpdate);

            return Json(new { success = true });
        }

        /// <summary>
        /// Adds finalized Treatment Record to Invoice
        /// </summary>
        /// <param name="recordId">Id of Treatment Record</param>
        /// <returns>
        /// returns a RedirectToAction to /Invoice/Create with Invoice Id
        /// </returns>
        [HttpPost]
        public IActionResult AddToInvoice(int recordId)
        {
            var record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == recordId);
            record.DateCreated = DateTime.Now;

            if (record.TreatmentId == null)
            {
                TempData["error"] = "A treatment must be selected to save";
                return RedirectToAction("UpsertTreatment", new { id = recordId });
            }else if (record.Dentists == null)
            {
                TempData["error"] = "A Dentist must be selected to save";
                return RedirectToAction("UpsertTreatment", new { id = recordId });
            }

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Added Treatment Record to Invoice", user.Id, user.ClinicId, SD.AuditCreate);

            TempData["success"] = "The treatment record has been added to the invoice.";

            return RedirectToAction("Create", new { invoiceId = record.InvoiceId });
        }

        /// <summary>
        /// Deletes treatment record draft
        /// </summary>
        /// <param name="recordId">Id of treatment record</param>
        /// <returns>
        /// returns a RedirectToAction to /Invoice/Create with Invoice Id
        /// </returns>
        [HttpPost]
        public IActionResult DiscardTreatmentRecord(int recordId)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == recordId);
            int? invoiceId = record.InvoiceId;

            _unitOfWork.TreatmentRecord.Remove(record);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Discarded Treatment Record from Invoice", user.Id, user.ClinicId, SD.AuditDelete);

            TempData["success"] = "Successfully discarded treatment record.";

            return RedirectToAction("Create", new { invoiceId = invoiceId });
        }

        /// <summary>
        /// Generates an Invoice Pdf for the user to download. Uses PuppeteerSharp.
        /// </summary>
        /// <param name="id">Id of Invoice</param>
        /// <returns>
        /// Returns the generated Invoice Pdf file of type FileStreamResult.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            // Solution 1: Only works on Chrome headless
            // The following code creates a headless browser
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            // Creates a new browser page
            await using var page = await browser.NewPageAsync();

            // The url that points to the Invoice/GetInvoiceTemplate method
            string? url = Url.Action("GetInvoiceTemplate", "Invoice", new { area = "Clinic", id = id });

            // The headless browser goes to the url and then loads the content of the page
            await page.GoToAsync($"{Request.Scheme}://{Request.Host}{url}");

            // This creates the content of the pdf based from the
            // page.GoToAsync()
            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            var file = File(pdfContent, "application/pdf", $"Invoice-{id}.pdf");

            var user = GetCurrentUser();
            InsertLog("Print Invoice", user.Id, user.ClinicId, "Generated a record as a pdf file.");

            // This returns the file (the invoice in pdf) to the browser
            // for the user to download and open
            return File(pdfContent, "application/pdf", $"Invoice-{id}.pdf");

            // Solution 2: Once you start testing with other browsers,
            // you might want to change the invoice generation to
            // taking a screenshot with page.ScreenshotAsync(outputFile)
        }

        /// <summary>
        /// Generates an Invoice Pdf of type byte array. 
        /// Used in EmailInvoice method.
        /// </summary>
        /// <param name="id">Id of Invoice</param>
        /// <returns>
        /// Returns the generated Invoice Pdf file of type byte array.
        /// </returns>
        [HttpGet]
        public async Task<byte[]> CreateInvoiceFile(int id)
        {
            byte[]? data = null;

            if (AuthorizeAccess() == false) return data;

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            await using var page = await browser.NewPageAsync();

            string? url = Url.Action("GetInvoiceTemplate", "Invoice", new { area = "Clinic", id = id });

            await page.GoToAsync($"{Request.Scheme}://{Request.Host}{url}");

            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            using (MemoryStream ms = new MemoryStream())
            {
                pdfContent.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Generates an Invoice Pdf to be sent as an attachment in an email.
        /// </summary>
        /// <param name="id">Id of patient</param>
        /// <returns>
        /// Returns a RedirectToAction Index after sending email.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> EmailInvoice(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            int patientId = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == id).PatientId;
            string? patientEmail = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == patientId).Email;

            var file = await CreateInvoiceFile(id);

            EmailSender emailSender = new EmailSender();
            var result = emailSender.EmailAttachmentAsync(patientEmail, "Invoice", "Here is your invoice.", file);
            if (result.IsCompletedSuccessfully)
            {
                TempData["success"] = "Invoice emailed successfully!";
                var user = GetCurrentUser();
                InsertLog("Sent an invoice through email", user.Id, user.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "There was a problem sending your email, please try again later.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Displays the selected invoice. Used in Print method.
        /// </summary>
        /// <param name="id">Id of Invoice</param>
        /// <returns> Returns view of InvoiceTemplate with a ViewModel </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetInvoiceTemplate(int id)
        {
            var vm = GetInvoice(id);
            return View("InvoiceTemplate", vm);
        }

        /// <summary>
        /// This records the payment details.
        /// </summary>
        /// <param name="vm">ViewModel -> CollectPaymentVM</param>
        /// <returns>returns the Index page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CollectPayment(CollectPaymentVM vm)
        {
            if (ModelState.IsValid)
            {
                var invoice = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == vm.Invoice.Id);

                if (invoice == null)
                    return View("Error");

                invoice.PaymentMethodId = vm.Invoice.PaymentMethodId;
                invoice.AmountPaid = vm.Invoice.AmountPaid;
                if(invoice.AmountPaid < invoice.TotalAmount)
                {
                    TempData["error"] = "Amount paid can not be less than the total amount due.";
                    return RedirectToAction("Index");
                }
                invoice.Change = vm.Invoice.AmountPaid - invoice.TotalAmount;
                invoice.PaymentDate = DateTime.Now;
                invoice.PaymentStatusId = 1;

                _unitOfWork.Invoice.Update(invoice);
                _unitOfWork.Save();

                var user = GetCurrentUser();
                InsertLog("Collect Payment", user.Id, user.ClinicId, SD.AuditUpdate);

                TempData["success"] = "Payment was successfully collected!";

                return RedirectToAction("Index");
            }

            return View("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult LoadCreateInvoiceForm()
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();
            InsertLog("View Create Invoice Form", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_CreateInvoiceForm");
        }

        [HttpGet]
        public IActionResult LoadInvoice(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            InvoiceVM vm = GetInvoice(id);

            var user = GetCurrentUser();
            InsertLog("View Invoice", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_Invoice", vm);
        }

        [HttpGet]
        public IActionResult LoadCollectPaymentForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            CollectPaymentVM? vm = new CollectPaymentVM()
            {
                Invoice = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == id),
                PaymentMethods = _unitOfWork.PaymentMethod.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Method,
                    Value = x.Id.ToString()
                })
            };

            return PartialView("_CollectPaymentForm", vm);
        }

        [HttpGet]
        public IActionResult GetAllPatients()
        {
            if (AuthorizeAccess() == false) return View("Error");

            var user = GetCurrentUser();

            // Get a list of patients' ids from the user's clinic.
            IEnumerable<ClinicPatient> clinicPatients = _unitOfWork.ClinicPatient.GetAll(x => x.ClinicId == user.ClinicId);

            // Get a list of patients and their decrypted data.
            List<AutocompletePatientVM> patientsFromDb = new List<AutocompletePatientVM>();
            foreach (ClinicPatient obj in clinicPatients)
            {
                Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == obj.PatientId && x.IsArchived == false);
                if(patient == null) continue;
                patientsFromDb.Add(new AutocompletePatientVM()
                {
                    Id = obj.PatientId,
                    Name = AesOperation.DecryptString(patient.LastName) + ", " + AesOperation.DecryptString(patient.FirstName) + " " + AesOperation.DecryptString(patient.MiddleName)
                });
            }

            return Json(new { data = patientsFromDb });
        }

        [HttpGet]
        public string GetAllInvoice(string status)
        {
            if (AuthorizeAccess() == false) return String.Empty;

            ApplicationUser user = GetCurrentUser();

            List<Invoice> invoice = _unitOfWork.Invoice.GetAll(x => x.ClinicId == user.ClinicId, includeProperties: "PaymentStatus").ToList();
            
            List<InvoiceListVM> vm = new List<InvoiceListVM>();
            Patient patient = new Patient();

            foreach(var item in invoice)
            {
                patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == item.PatientId);

                vm.Add(new InvoiceListVM()
                {
                    Id = item.Id,
                    InvoiceDate = item.InvoiceDate,
                    PatientName = AesOperation.DecryptString(patient.LastName) + ", " + AesOperation.DecryptString(patient.FirstName) + " " + AesOperation.DecryptString(patient.MiddleName),
                    TotalAmount = item.TotalAmount,
                    PaymentStatus = item.PaymentStatus.Status,
                    PaymentMethod = "NULL",
                    InvoiceStatus = item.InvoiceStatus,
                    IsArchived = item.IsArchived,
                });
            }

            switch (status)
            {
                case "Archived":
                    vm = vm.Where(x => x.IsArchived == true).ToList();
                    break;
                case "Active":
                    vm = vm.Where(x => x.IsArchived == false).ToList();
                    break;
                case "Paid":
                    vm = vm.Where(x => x.PaymentStatus == "Paid" && x.IsArchived == false).ToList();
                    break;
                case "Pending":
                    vm = vm.Where(x => x.PaymentStatus == "Pending" && x.IsArchived == false).ToList();
                    break;
                case "Created":
                    vm = vm.Where(x => x.InvoiceStatus == "Created" && x.IsArchived == false).ToList();
                    break;
                case "Draft":
                    vm = vm.Where(x => x.InvoiceStatus == "Draft" && x.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(vm, jsonSettings);
        }

        [HttpGet]
        public IActionResult GetAllTreatments()
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser user = GetCurrentUser();
            var treatmentList = _unitOfWork.Treatment.GetAll(includeProperties: "TreatmentType").Where(x => x.IsArchived == false && x.ClinicId == user.ClinicId);
            return Json(new { data = treatmentList });
        }

        [HttpGet]
        public IActionResult LoadAddTreatmentForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            ViewData["Id"] = id;
            var user = GetCurrentUser();
            InsertLog("View Add Treatment Form", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_AddTreatmentForm");
        }

        [HttpGet]
        public IActionResult LoadAddToothForm(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            ViewData["Id"] = id;
            var user = GetCurrentUser();
            InsertLog("View Add Tooth Form", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_AddToothForm");
        }

        /// <summary>
        /// Increases the quantity of a treatment and updates the TotalPrice 
        /// at the same time.
        /// </summary>
        /// <param name="id">Id of TreatmentRecord</param>
        [HttpPost]
        public void IncrementQuantity(int id)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            Treatment? treatment = _unitOfWork.Treatment.GetFirstOrDefault(x => x.Id == record.TreatmentId);

            record.Quantity += 1;
            record.TotalPrice = treatment.Price * record.Quantity;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Increased quantity of treatment", user.Id, user.ClinicId, SD.AuditUpdate);

            TempData["success"] = "The quantity was successfully increased.";
        }

        /// <summary>
        /// Decreases the quantity of a treatment and updates the TotalPrice 
        /// at the same time.
        /// </summary>
        /// <param name="id">Id of TreatmentRecord</param>
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

                var user = GetCurrentUser();
                InsertLog("Decreased quantity of treatment", user.Id, user.ClinicId, SD.AuditUpdate);

                TempData["success"] = "The quantity was successfully reduced.";
            }
        }

        /// <summary>
        /// Adds tooth no./s to the treatment record.
        /// </summary>
        /// <param name="id">Id of TreatmentRecord</param>
        /// <param name="data">The tooth numbers inside a string array</param>
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

            var user = GetCurrentUser();
            InsertLog("Add tooth numbers to treatment record", user.Id, user.ClinicId, SD.AuditUpdate);

            TempData["success"] = "Tooth numbers have been added to the record.";
        }

        /// <summary>
        /// Adds tooth no./s to the treatment record.
        /// </summary>
        /// <param name="id">Id of TreatmentRecord</param>
        /// <param name="data">The tooth numbers in type string</param>
        [HttpPost]
        public void AddTeeth(int id, string data)
        {
            TreatmentRecord? record = _unitOfWork.TreatmentRecord.GetFirstOrDefault(x => x.Id == id);
            record.ToothNumbers = data;

            _unitOfWork.TreatmentRecord.Update(record);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Added Teeth to Treatment Record", user.Id, user.ClinicId, SD.AuditUpdate);

            TempData["success"] = "Tooth numbers have been added to the record.";
        }

        [HttpGet]
        public async Task<IActionResult> LoadAddDentistFormAsync(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            ApplicationUser? currentUser = GetCurrentUser();

            List<ApplicationUser>? clinicUsers = _userManager.Users.Where(x => x.ClinicId == currentUser.ClinicId && x.IsArchived==false).ToList();

            List<ApplicationUser>? dentists = new List<ApplicationUser>();

            foreach (ApplicationUser user in clinicUsers)
            {
                if ((await _userManager.IsInRoleAsync(user, SD.Role_Dentist)) || (await _userManager.IsInRoleAsync(user, SD.Role_Owner)) && (user.IsArchived==false))
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

        /// <summary>
        /// Adds dentist/s to the treatment record.
        /// </summary>
        /// <param name="id">Id of TreatmentRecord</param>
        /// <param name="data">Dentist/s inside a string array</param>
        [HttpPost]
        public void AddDentists(int id, string[] data)
        {
            string dentists = "";
            if (data.Length != 0)
            {
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

                var user = GetCurrentUser();
                InsertLog("Added dentists to treatment record", user.Id, user.ClinicId, SD.AuditUpdate);
                TempData["success"] = "Dentist/s have been added to the record.";
            }
            else
            {
                TempData["error"] = "At least one dentist must be selected";
            }
        }

        [HttpPost]
        public IActionResult Archive(int? id)
        {
            var obj = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == id);
            
            if (obj == null)
            {
                return Json(new { success = false, message = "Invoice does not exist." });
            }

            obj.IsArchived = true;
            _unitOfWork.Invoice.Update(obj);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Archived Invoice", user.Id, user.ClinicId, SD.AuditArchive);

            return Json(new { success = true, message = "Archived successfully!" });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (!User.IsInRole(SD.Role_Owner))
            {
                TempData["error"] = "You are not authorized to delete this invoice.";
                return RedirectToAction("Index");
            }

            var obj = _unitOfWork.Invoice.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Invoice does not exist." });
            }

            var treatmentRecords = _unitOfWork.TreatmentRecord.GetAll(x => x.InvoiceId == id);

            if (treatmentRecords != null)
            {
                _unitOfWork.TreatmentRecord.RemoveRange(treatmentRecords);
                _unitOfWork.Save();
            }

            _unitOfWork.Invoice.Remove(obj);
            _unitOfWork.Save();

            var user = GetCurrentUser();
            InsertLog("Delete Invoice", user.Id, user.ClinicId, SD.AuditDelete);

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

        #region HELPER FUNCTIONS

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return  _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
        }

        /// <summary>
        /// Appends the @stringToAppend with a comma at the end to the 
        /// @mainString. If it is the @lastSequence in the array, it will not 
        /// include a comma at the end.
        /// </summary>
        /// <param name="mainString">The main string.</param>
        /// <param name="stringToAppend">
        /// The string to append to the main string
        /// </param>
        /// <param name="lastSequence">
        /// Set as true if this method will be used on the last sequence of an 
        /// array.
        /// </param>
        /// <returns>
        /// Returns the appended string.
        /// </returns>
        public string AppendString(string mainString, string stringToAppend, bool lastSequence = false)
        {
            if (lastSequence)
                return mainString + stringToAppend;

            return mainString + stringToAppend + ", ";
        }

        public InvoiceVM GetInvoice(int id)
        {
            InvoiceVM vm = new InvoiceVM();

            vm.Invoice = _unitOfWork.Invoice.GetFirstOrDefault(x => x.Id == id, includeProperties: "PaymentStatus,PaymentMethod");

            ApplicationUser? user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == vm.Invoice.UserId);
            vm.UserName = AesOperation.DecryptString(user.LastName) + ", " + AesOperation.DecryptString(user.FirstName) + " " + AesOperation.DecryptString(user.MiddleName);

            Patient? patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == vm.Invoice.PatientId);
            vm.PatientName = AesOperation.DecryptString(patient.LastName) + ", " + AesOperation.DecryptString(patient.FirstName) + " " + AesOperation.DecryptString(patient.MiddleName);

            VitasysEHR.Models.Clinic? clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == vm.Invoice.ClinicId);
            vm.ClinicName = AesOperation.DecryptString(clinic.Name);
            vm.ClinicAddress = AesOperation.DecryptString(clinic.Address);
            vm.ClinicCity = AesOperation.DecryptString(clinic.City);
            vm.ClinicProvince = AesOperation.DecryptString(clinic.Province);
            vm.ClinicZipCode = AesOperation.DecryptString(clinic.ZipCode);
            vm.ClinicOfficePhone = AesOperation.DecryptString(clinic.OfficePhone);
            vm.ClinicMobilePhone = AesOperation.DecryptString(clinic.MobilePhone);
            vm.ClinicEmailAddress = AesOperation.DecryptString(clinic.EmailAddress);

            vm.TreatmentRecordList = _unitOfWork.TreatmentRecord.GetAll(x => x.InvoiceId == vm.Invoice.Id, includeProperties: "Treatment").ToList();

            return vm;
        }

        /// <summary>
        /// Inserts the activity as an audit log.
        /// </summary>
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
