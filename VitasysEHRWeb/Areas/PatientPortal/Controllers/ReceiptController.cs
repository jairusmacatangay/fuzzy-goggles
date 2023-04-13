using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

    public class ReceiptController : Controller
    {
        public decimal BalanceDue { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        public ReceiptViewVM ReceiptViewVM { get; set; }
        public ReceiptController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            ReceiptViewVM = new ReceiptViewVM()
            {
                Invoices = _unitOfWork.Invoice.GetAll(x => x.PatientId == user.Id)
            };
            foreach (var bill in ReceiptViewVM.Invoices)
            {
                if (bill.PaymentStatusId != 1)
                {
                    ReceiptViewVM.BalanceDue += bill.TotalAmount;
                }
            }

            InsertLog("Viewed Receipts List", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "receipts";
            return View(ReceiptViewVM);
        }
        
        public IActionResult ViewReceipt(int receiptId)
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            
            if (user == null)
                return RedirectToAction("Login", "Account");

            ReceiptViewVM? ReceiptViewVM = new ReceiptViewVM()
            {
                Invoice = _unitOfWork.Invoice.GetFirstOrDefault(i => i.Id == receiptId, includeProperties: "Clinic,Patient,PaymentStatus,PaymentMethod"),
                TreatmentRecords = _unitOfWork.TreatmentRecord.GetAll(x => x.InvoiceId == receiptId, includeProperties: "Invoice,Treatment")
                
            };

            //Clinic Information Decryption
            ReceiptViewVM.ClinicName = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.Name);
            ReceiptViewVM.Address = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.Address);
            ReceiptViewVM.City = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.City);
            ReceiptViewVM.Province = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.Province);
            ReceiptViewVM.ZipCode = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.ZipCode);
            ReceiptViewVM.EmailAddress = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.EmailAddress);
            ReceiptViewVM.MobilePhone = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.MobilePhone);
            ReceiptViewVM.OfficePhone = AesOperation.DecryptString(ReceiptViewVM.Invoice.Clinic.OfficePhone);
            
            //Patient Information Decryption
            ReceiptViewVM.PatientName = AesOperation.DecryptString(ReceiptViewVM.Invoice.Patient.FirstName) + " " +
                AesOperation.DecryptString(ReceiptViewVM.Invoice.Patient.MiddleName)+ " " +
                AesOperation.DecryptString(ReceiptViewVM.Invoice.Patient.LastName);

            if(ReceiptViewVM.Invoice.PaymentMethod == null)
            {
                ReceiptViewVM.PaymentMethod = "Not Paid";
            }else if (ReceiptViewVM.Invoice.PaymentMethod != null)
            {
                ReceiptViewVM.PaymentMethod = ReceiptViewVM.Invoice.PaymentMethod.Method;
            }

            ReceiptViewVM.InvoiceDate = ReceiptViewVM.Invoice.InvoiceDate.Value.ToString("MM/dd/yyyy");
            if(ReceiptViewVM.Invoice.PaymentDate == null)
            {
                ReceiptViewVM.PaymentDate = "Not Paid";
            }
            else
            {
                ReceiptViewVM.PaymentDate = ReceiptViewVM.Invoice.PaymentDate.Value.ToString("MM/dd/yyyy");
            }
           

            InsertLog("Viewed Receipt", user.Id, SD.AuditView);

            ViewData["CurrentPage"] = "receipts";
            return View(ReceiptViewVM);
        }

        #region API Calls
        [HttpGet]
        public string GetAll()
        {
            Patient? user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            var invoiceFromDb = _unitOfWork.Invoice.GetAll(x => x.PatientId == user.Id && x.IsArchived == false && x.InvoiceStatus == "Created", includeProperties: "Clinic,PaymentStatus");
            List<ReceiptVM> invoices = new List<ReceiptVM>();

            foreach (var obj in invoiceFromDb)
            {

                invoices.Add(new ReceiptVM()
                {
                    Id = obj.Id,
                    InvoiceDate = obj.InvoiceDate,
                    Clinic = AesOperation.DecryptString(obj.Clinic.Name),
                    PaymentStatus = obj.PaymentStatus.Status,
                    TotalAmount = obj.TotalAmount,
                }
                );
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(invoices, jsonSettings);
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
