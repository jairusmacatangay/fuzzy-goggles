using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;
using VitasysEHRWeb.Utility;

namespace VitasysEHRWeb.Areas.PatientRecords.Controllers
{
    [Area("PatientRecords")]
    public class MedicalHistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalHistoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public IActionResult Index(int id)
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
            MedicalHistoryVM vm = new MedicalHistoryVM();

            vm.MedicalHistory = _unitOfWork.MedicalHistory.GetFirstOrDefault(x => x.PatientId == id);

            if (vm.MedicalHistory != null)
            {
                vm.Allergy = _unitOfWork.Allergy.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.AllergyId);
                vm.ReviewOfSystem = _unitOfWork.ReviewOfSystem.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.ROSId);
            }

            Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
            vm.Gender = AesOperation.DecryptString(patient.Gender);

            ViewData["PatientId"] = patient.Id;
            ViewData["Gender"] = vm.Gender;

            
            InsertLog("View Medical History", user.Id, user.ClinicId, SD.AuditView);

            ViewData["CurrentPage"] = "medicalHistory";

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public IActionResult Create(MedicalHistoryVM obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ReviewOfSystem.Add(obj.ReviewOfSystem);
                _unitOfWork.Allergy.Add(obj.Allergy);
                _unitOfWork.Save();

                obj.MedicalHistory.PatientId = obj.PatientId;
                obj.MedicalHistory.AllergyId = obj.Allergy.Id;
                obj.MedicalHistory.ROSId = obj.ReviewOfSystem.Id;

                _unitOfWork.MedicalHistory.Add(obj.MedicalHistory);
                _unitOfWork.Save();

                ApplicationUser user = GetCurrentUser();
                InsertLog("Create Medical History", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Medical history created successfully";

                ViewData["PatientId"] = obj.PatientId;
                ViewData["Gender"] = obj.Gender;

                return RedirectToAction("Index", new { id = obj.PatientId });
            }
            return View(obj);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist)]
        public IActionResult Update(string? id)
        {
            int decryptedId = Int32.Parse(AesOperation.DecryptString(id, true));

            MedicalHistoryVM vm = new();
            vm.MedicalHistory = _unitOfWork.MedicalHistory.GetFirstOrDefault(x => x.PatientId == decryptedId);

            if (!vm.MedicalHistory.IsEditable)
            {
                TempData["error"] = "Medical history cannot be edited.";
                return RedirectToAction("Index", new { id = decryptedId });
            }

            Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == decryptedId);
            vm.PatientId = patient.Id;
            vm.Gender = AesOperation.DecryptString(patient.Gender);
            vm.BloodTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "O+", Value = "O+" },
                new SelectListItem { Text = "O-", Value = "O-" },
                new SelectListItem { Text = "A+", Value = "A+" },
                new SelectListItem { Text = "A-", Value = "A-" },
                new SelectListItem { Text = "B+", Value = "B+" },
                new SelectListItem { Text = "B-", Value = "B-" },
                new SelectListItem { Text = "AB+", Value = "AB+" },
                new SelectListItem { Text = "AB-", Value = "AB-" },
            };
            
            vm.Allergy = _unitOfWork.Allergy.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.AllergyId);
            vm.ReviewOfSystem = _unitOfWork.ReviewOfSystem.GetFirstOrDefault(x => x.Id == vm.MedicalHistory.ROSId);

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Edit Medical History Form", user.Id, user.ClinicId, SD.AuditView);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist)]
        public IActionResult Update(MedicalHistoryVM obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    obj.MedicalHistory.IsEditable = false;

                    _unitOfWork.Allergy.Update(obj.Allergy);
                    _unitOfWork.ReviewOfSystem.Update(obj.ReviewOfSystem);
                    _unitOfWork.MedicalHistory.Update(obj.MedicalHistory);
                    _unitOfWork.Save();

                    ApplicationUser user = GetCurrentUser();
                    InsertLog("Update Medical History", user.Id, user.ClinicId, SD.AuditUpdate);

                    TempData["success"] = "Medical History updated successfully";

                    ViewData["PatientId"] = obj.PatientId;
                    ViewData["Gender"] = obj.Gender;

                    Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == obj.PatientId);
                    string name = $"{AesOperation.DecryptString(patient.FirstName)} {AesOperation.DecryptString(patient.LastName)}";

                    HttpContext.Session.Remove(SD.SessionKeyPatientName);
                    HttpContext.Session.Remove(SD.SessionKeyPatientId);
                    HttpContext.Session.Remove(SD.SessionKeyPatientProfPicUrl);

                    HttpContext.Session.SetString(SD.SessionKeyPatientName, name);
                    HttpContext.Session.SetInt32(SD.SessionKeyPatientId, patient.Id);

                    if (!string.IsNullOrEmpty(patient.ProfPicUrl))
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, AesOperation.DecryptString(patient.ProfPicUrl));
                    else
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, "/img/patients/prof-pic-placeholder.png");

                    return RedirectToAction("Index", new { id = obj.PatientId });
                }

                return View(obj);
            }
            catch (DbUpdateException)
            {
                TempData["error"] = "System failed to update record. Please try again later.";
                return RedirectToAction("Index", new { id = obj.PatientId });
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequestAsync(int Id, string reason)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(reason))
                {
                    TempData["error"] = "Reason field cannot be empty";
                    return RedirectToAction("Index", new { id = Id });
                }

                var patient = _unitOfWork.Patient.GetFirstOrDefault(g => g.Id == Id);
                if (patient == null || patient.Email == null)
                {
                    TempData["error"] = "Patient does not exist!";
                    return RedirectToAction("Index", new { id = Id });
                }

                // Create data to include as URL parameter
                ApplicationUser currentUser = GetCurrentUser();
                string? urlParamId = AesOperation.EncryptString(Id.ToString(), true);
                string? urlParamEmail = AesOperation.EncryptString(currentUser.Email, true);

                // Create approve and deny urls
                string? approveUrl = Url.Action("ProcessResponse", "MedicalHistory", new
                {
                    area = "PatientRecords",
                    id = urlParamId,
                    email = urlParamEmail,
                    response = "EditApproved"
                });
                string? denyUrl = Url.Action("ProcessResponse", "MedicalHistory", new
                {
                    area = "PatientRecords",
                    id = urlParamId,
                    email = urlParamEmail,
                    response = "EditDenied"
                });

                // Decrypt current user info
                var firstname = AesOperation.DecryptString(currentUser.FirstName);
                var lastname = AesOperation.DecryptString(currentUser.LastName);

                // Send email
                EmailSender emailSender = new();
                await emailSender.SendEmailAsync(
                    patient.Email,
                    $"Dr. {firstname} {lastname} would like to request to edit your medical history",
                    $"<div>Dr. {firstname} {lastname} is requesting to edit your medical history.</div><br />" +
                    "<div><strong>Reason:</strong></div>" +
                    $"<div>{reason.Trim()}</div><br />" +
                    "<div><strong>Approve Request:</strong></div>" +
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveUrl}'>approve</a><br /><br />" +
                    "<div><strong>Deny Request:</strong></div>" +
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyUrl}'>deny</a>");

                // Display success message
                TempData["success"] = "Edit medical history request was successfully sent!";

                return RedirectToAction("Index", new { id = Id });
            }
            catch
            {
                TempData["error"] = "An unexpected error happened";
                return RedirectToAction("Index", new { id = Id });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessResponseAsync(string? id, string? email, string response)
        {
            try
            {
                // Decrypt data
                int decryptedId = Int32.Parse(AesOperation.DecryptString(id, true));
                string? decryptedEmail = AesOperation.DecryptString(email, true);
                if (decryptedId < 0 || decryptedEmail == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                // Get patient
                var patient = _unitOfWork.Patient.GetFirstOrDefault(g => g.Id == decryptedId);
                if (patient == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                // Get patient's medical history
                var medicalHistory = _unitOfWork.MedicalHistory.GetFirstOrDefault(g => g.PatientId == decryptedId);
                if (medicalHistory == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                // EmailSender instance
                EmailSender emailSender = new();

                // Decrypt first and last name
                string? firstName = AesOperation.DecryptString(patient.FirstName);
                string? lastName = AesOperation.DecryptString(patient.LastName);

                if (response == "EditApproved")
                {
                    // Create edit url
                    string? editUrl = Url.Action("Update", "MedicalHistory", new { area = "PatientRecords", id });

                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"Patient {firstName} {lastName} has approved your edit request",
                        $"<div>Patient <strong>{firstName} {lastName}</strong> has approved your edit request</div><br />" +
                        $"Click here to <a href='{Request.Scheme}://{Request.Host}{editUrl}'>edit</a> patient's medical history.<br /><br />");

                    // This will make the patient's medical history editable
                    medicalHistory.IsEditable = true;

                    // Update medical history record
                    _unitOfWork.MedicalHistory.Update(medicalHistory);
                    _unitOfWork.Save();
                }
                else if (response == "EditDenied")
                {
                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has denied your edit request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has denied your edit request." +
                        "You won't be able to edit the user's record.</div>");
                }
                else
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                SetResponseMessage(response);
                return View();
            }
            catch (Exception)
            {
                SetResponseMessage("UnexpectedError");
                return View();
            }
        }

        private void SetResponseMessage(string response)
        {
            ViewData["header"] = ResponseMessage.HEADER[response];
            ViewData["message"] = ResponseMessage.MESSAGE[response];
            ViewData["icon"] = ResponseMessage.ICON[response];
        }

        #region API CALLS
        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public IActionResult LoadCreateForm(int id)
        {
            MedicalHistoryVM vm = new MedicalHistoryVM();
            
            Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
            vm.PatientId = patient.Id;
            vm.Gender = AesOperation.DecryptString(patient.Gender);

            vm.BloodTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "O+", Value = "O+" },
                new SelectListItem { Text = "O-", Value = "O-" },
                new SelectListItem { Text = "A+", Value = "A+" },
                new SelectListItem { Text = "A-", Value = "A-" },
                new SelectListItem { Text = "B+", Value = "B+" },
                new SelectListItem { Text = "B-", Value = "B-" },
                new SelectListItem { Text = "AB+", Value = "AB+" },
                new SelectListItem { Text = "AB-", Value = "AB-" },
            };

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Create Medical History Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_CreateMedicalHistoryForm", vm);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist)]
        public IActionResult LoadEditRequestForm(int id)
        {
            var objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
            };

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Edit Medical History Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditRequestForm", patient);
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