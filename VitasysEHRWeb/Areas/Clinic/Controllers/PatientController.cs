using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;
using VitasysEHRWeb.Service.IService;
using VitasysEHRWeb.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    public class PatientController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public PatientController(
            IUnitOfWork unitOfWork, 
            IUserService userService,
            IWebHostEnvironment hostEnvironment, 
            SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _hostEnvironment = hostEnvironment;
            _signInManager = signInManager;
        }

        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userService.GetCurrentUser();
            if (user.AdminVerified == "Pending")
            {
                return RedirectToPage("/Account/Manage/VerifiedError", new { area = "Identity" });
            }
            else if (user.AdminVerified == "Denied")
            {
                return RedirectToPage("/Account/Manage/PermitReverify", new { area = "Identity" });
            }


            if (user.FirstName == null) return RedirectToPage("/Account/Manage/Profile", new { area = "Identity" });

            if (user.ClinicId == null) return RedirectToAction("CreateClinic", "Account", new { area = "Clinic" });

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null) return RedirectToAction("Index", "Subscription", new { area = "Clinic" });

            if (User.IsInRole(SD.Role_Owner))
            {
                string? message = GetSubscriptionMessage(subscription);
                if (message != null) TempData["SubscriptionAlertMessage"] = message;
            }

            SetSubscriptionSession(subscription);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> CreateAsync(Patient obj, IFormFile? file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            if (ModelState.IsValid)
            {
                var user = await _userService.GetCurrentUser();
                var clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                var userCheck = _unitOfWork.Patient.GetFirstOrDefault(x => x.Email == obj.Email);
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (userCheck != null)
                {
                    var clinicPatients = _unitOfWork.ClinicPatient.GetFirstOrDefault(x => x.ClinicId == user.ClinicId && x.PatientId == userCheck.Id);
                    if (clinicPatients != null)
                    {
                        TempData["error"] = "Patient already exists!";
                        return RedirectToAction("Index");
                    }
                    ClinicPatient existingPatient = new ClinicPatient()
                    {
                        PatientId = userCheck.Id,
                        ClinicId = clinic.Id
                    };
                    _unitOfWork.ClinicPatient.Add(existingPatient);
                    _unitOfWork.Save();
                    InsertLog("Added Existing Patient", user.Id, user.ClinicId, SD.AuditCreate);
                    return RedirectToAction("Index");
                }
                // Encrypt inputs
                Patient patient = new Patient()
                {
                    FirstName = AesOperation.EncryptString(obj.FirstName),
                    MiddleName = AesOperation.EncryptString(obj.MiddleName),
                    LastName = AesOperation.EncryptString(obj.LastName),
                    DOB = AesOperation.EncryptString(obj.DOB),
                    Gender = AesOperation.EncryptString(obj.Gender),
                    Address = AesOperation.EncryptString(obj.Address),
                    City = AesOperation.EncryptString(obj.City),
                    Province = AesOperation.EncryptString(obj.Province),
                    ZipCode = AesOperation.EncryptString(obj.ZipCode),
                    Email = obj.Email,
                    HomeNumber = AesOperation.EncryptString(obj.HomeNumber),
                    MobileNumber = AesOperation.EncryptString(obj.MobileNumber)
                };

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(wwwRootPath, @"img\patients");
                    var extension = Path.GetExtension(file.FileName);

                    if (!imageExtensions.Contains(extension))
                    {
                        TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.ProfPicUrl = @"\img\patients\" + fileName + extension;
                        patient.ProfPicUrl = AesOperation.EncryptString(obj.ProfPicUrl);
                    }
                }

                _unitOfWork.Patient.Add(patient);
                _unitOfWork.Save();

                ClinicPatient clinicPatient = new ClinicPatient()
                {
                    ClinicId = clinic.Id,
                    PatientId = patient.Id
                };

                _unitOfWork.ClinicPatient.Add(clinicPatient);
                _unitOfWork.Save();

                InsertLog("Create Patient", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Patient created successfully";

                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> UpdateAsync(Patient obj, IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            ModelState.ClearValidationState("file");
            ModelState.MarkFieldValid("file");
            if (ModelState.IsValid) { 
            //var userCheck = _unitOfWork.Patient.GetFirstOrDefault(x => x.Email == obj.Email);

                //if(userCheck != null)
                //{
                //    TempData["error"] = "Patient with this email already exists";
                //    return RedirectToAction("Index");
                //}
                Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == obj.Id);

                // Update only modified fields
                if (obj.FirstName != AesOperation.DecryptString(objFromDb.FirstName)) objFromDb.FirstName = AesOperation.EncryptString(obj.FirstName);
                if (obj.MiddleName != AesOperation.DecryptString(objFromDb.MiddleName)) objFromDb.MiddleName = AesOperation.EncryptString(obj.MiddleName);
                if (obj.LastName != AesOperation.DecryptString(objFromDb.LastName)) objFromDb.LastName = AesOperation.EncryptString(obj.LastName);
                if (obj.DOB != AesOperation.DecryptString(objFromDb.DOB)) objFromDb.DOB = AesOperation.EncryptString(obj.DOB);
                if (obj.Gender != AesOperation.DecryptString(objFromDb.Gender)) objFromDb.Gender = AesOperation.EncryptString(obj.Gender);
                if (obj.Address != AesOperation.DecryptString(objFromDb.Address)) objFromDb.Address = AesOperation.EncryptString(obj.Address);
                if (obj.City != AesOperation.DecryptString(objFromDb.City)) objFromDb.City = AesOperation.EncryptString(obj.City);
                if (obj.Province != AesOperation.DecryptString(objFromDb.Province)) objFromDb.Province = AesOperation.EncryptString(obj.Province);
                if (obj.ZipCode != AesOperation.DecryptString(objFromDb.ZipCode)) objFromDb.ZipCode = AesOperation.EncryptString(obj.ZipCode);
                if (obj.Email != objFromDb.Email) objFromDb.Email = obj.Email;
                if (obj.HomeNumber != AesOperation.DecryptString(objFromDb.HomeNumber)) objFromDb.HomeNumber = AesOperation.EncryptString(obj.HomeNumber);
                if (obj.MobileNumber != AesOperation.DecryptString(objFromDb.MobileNumber)) objFromDb.MobileNumber = AesOperation.EncryptString(obj.MobileNumber);

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(wwwRootPath, @"img\patients");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.ProfPicUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.ProfPicUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.ProfPicUrl = @"\img\patients\" + fileName + extension;
                    objFromDb.ProfPicUrl = AesOperation.EncryptString(obj.ProfPicUrl);
                }

                objFromDb.IsEditable = false;
                objFromDb.LastModified = DateTime.Now;

                TempData["success"] = "Patient updated successfully";

                _unitOfWork.Patient.Update(objFromDb);
                _unitOfWork.Save();


                var user = await _userService.GetCurrentUser();
                InsertLog("Update Patient", user.Id, user.ClinicId, SD.AuditUpdate);

                return RedirectToAction(nameof(IndexAsync));
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> EditRequest(int? id, string reason)
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
                Patient patient = new Patient()
                {
                    Id = objFromDb.Id,
                    LastName = AesOperation.DecryptString(objFromDb.LastName),
                    Email = objFromDb.Email,
                };

                // Send email
                string? userId = AesOperation.EncryptString(user.Id, true);
                string? urlParamId = AesOperation.EncryptString(patient.Id.ToString(), true);
                string? approveCallbackUrl = Url.Action("ProcessResponse", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "EditApproved" });
                string? denyCallbackUrl = Url.Action("ProcessResponse", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "EditDenied" });
                EmailSender emailSender = new EmailSender();

                await emailSender.SendEmailAsync(
                    patient.Email,
                    "Request to Edit your Records",
                    $"<div>Hello Mr./Mrs. {patient.LastName}, {AesOperation.DecryptString(user.Clinic.Name)} would like to request permission to edit your health record </div><br />" +
                    "<div><strong>Reason:</strong></div>" + 
                    $"<div>{reason.Trim()}</div><br />" +
                     "<div><strong>Approve:</strong></div>" + 
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveCallbackUrl}'>approve</a><br /><br />" +
                     "<div><strong>Deny:</strong></div>" + 
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyCallbackUrl}'>deny</a>");


                TempData["success"] = "Edit patient request was successfully sent!";

                InsertLog("Edit Patient Request Email", user.Id, user.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> ArchiveRequest(int? id, string reason)
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
                Patient patient = new Patient()
                {
                    Id = objFromDb.Id,
                    LastName = AesOperation.DecryptString(objFromDb.LastName),
                    Email = objFromDb.Email,
                };

                // Send email
                string? userId = AesOperation.EncryptString(user.Id, true);
                string? urlParamId = AesOperation.EncryptString(patient.Id.ToString(), true);
                string? approveCallbackUrl = Url.Action("Archive", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "ArchiveApproved" });
                string? denyCallbackUrl = Url.Action("Archive", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "ArchiveDenied" });
                EmailSender emailSender = new EmailSender();

                await emailSender.SendEmailAsync(
                    patient.Email,
                    "Request to Archive your Records",
                    $"Hello Mr./Mrs. {patient.LastName}, {AesOperation.DecryptString(user.Clinic.Name)} would like to request permission to archive your health records, " +
                    $"with the following reasons:{reason} " + "\n" +
                    $"Click here if you would like to <a href='{Request.Scheme}://{Request.Host}{approveCallbackUrl}'>Approve</a> this request." +
                    $"If you have issues with this action please click here to <a href='{Request.Scheme}://{Request.Host}{denyCallbackUrl}'>Deny</a> this request.");


                TempData["success"] = "Archive patient request was successfully sent!";

                InsertLog("Archive Patient Request Email", user.Id, user.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> DeleteRequest(int? id, string reason)
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);
                Patient patient = new Patient()
                {
                    Id = objFromDb.Id,
                    LastName = AesOperation.DecryptString(objFromDb.LastName),
                    Email = objFromDb.Email,
                };

                // Send email
                string? userId = AesOperation.EncryptString(user.Id, true);
                string? urlParamId = AesOperation.EncryptString(patient.Id.ToString(), true);
                string? approveCallbackUrl = Url.Action("Delete", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "DeleteApproved" });
                string? denyCallbackUrl = Url.Action("Delete", "Patient", new { area = "Clinic", id = urlParamId, userid = userId, response = "DeleteDenied" });
                EmailSender emailSender = new EmailSender();

                await emailSender.SendEmailAsync(
                    patient.Email,
                    "Request to Delete your Records",
                    $"Hello Mr./Mrs. {patient.LastName}, {AesOperation.DecryptString(user.Clinic.Name)} would like to request permission to delete your health records, " +
                    $"with the following reasons:{reason} " + "\n" +
                    $"Click here if you would like to <a href='{Request.Scheme}://{Request.Host}{approveCallbackUrl}'>Approve</a> this request." +
                    $"If you have issues with this action please click here to <a href='{Request.Scheme}://{Request.Host}{denyCallbackUrl}'>Deny</a> this request.");
                TempData["success"] = "Delete patient request was successfully sent!";

                InsertLog("Delete Patient Request Email", user.Id, user.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> ProcessResponse(string response, string? id, string? userId)
        {
            try
            {
                // Send email to clinic
                userId = AesOperation.DecryptString(userId, true);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                var clinicEmail = AesOperation.DecryptString(user.Clinic.EmailAddress);
                id = AesOperation.DecryptString(id, true);

                EmailSender emailSender = new EmailSender();
                if (response == "EditApproved")
                {
                    if (_signInManager.IsSignedIn(User))
                    {
                        LogoutUser();
                        return RedirectToAction("ProcessResponse");
                    }

                    string? urlParamId = AesOperation.EncryptString(id.ToString(), true);
                    string? callbackUrl = Url.Action("Update", "Patient", new { area = "Clinic", id = urlParamId });

                    await emailSender.SendEmailAsync(
                    clinicEmail,
                    "Request to Edit Approved",
                    $"Your request has been approved by the patient, please <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>click here</a> to begin editing.");

                    var patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == Int32.Parse(id));
                    patient.IsEditable = true;
                    _unitOfWork.Patient.Update(patient);
                    _unitOfWork.Save();

                }
                else if (response == "EditDenied")
                {
                    if (_signInManager.IsSignedIn(User))
                    {
                        LogoutUser();
                        return RedirectToAction("ProcessResponse");
                    }
                    
                    string? urlParamId = AesOperation.EncryptString(user.Id.ToString(), true);
                    string? callbackUrl = Url.Action("Index", "Patient", new { area = "Clinic", id = urlParamId });

                    await emailSender.SendEmailAsync(
                    clinicEmail,
                    "Request to Edit Denied",
                    $"Your request has been denied by the patient, please <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>click here</a> to return to the application.");
                }

                response ??= "";

                if (ResponseMessage.HEADER.ContainsKey(response))
                {
                    ViewData["header"] = ResponseMessage.HEADER[response];
                    ViewData["message"] = ResponseMessage.MESSAGE[response];
                    ViewData["icon"] = ResponseMessage.ICON[response];
                }
                else
                {
                    ViewData["header"] = ResponseMessage.HEADER["Success"];
                    ViewData["message"] = ResponseMessage.MESSAGE["Success"];
                    ViewData["icon"] = ResponseMessage.ICON["Success"];
                }

                return View();
            }
            catch
            {
                return View("Error");
            }
        }


        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> UpdateAsync(string id)
        {
            try
            {
                int targetId = int.Parse(AesOperation.DecryptString(id, true));
                Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == targetId);

                if (!objFromDb.IsEditable)
                {
                    TempData["error"] = "User's record cannot be edited";
                    return RedirectToAction("Index");
                }

                Patient patient = new()
                {
                    Id = objFromDb.Id,
                    FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                    MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                    LastName = AesOperation.DecryptString(objFromDb.LastName),
                    DOB = AesOperation.DecryptString(objFromDb.DOB),
                    Email = objFromDb.Email,
                    HomeNumber = AesOperation.DecryptString(objFromDb.HomeNumber),
                    MobileNumber = AesOperation.DecryptString(objFromDb.MobileNumber),
                    Gender = AesOperation.DecryptString(objFromDb.Gender),
                    Address = AesOperation.DecryptString(objFromDb.Address),
                    City = AesOperation.DecryptString(objFromDb.City),
                    Province = AesOperation.DecryptString(objFromDb.Province),
                    ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                    ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
                };

                var user = await _userService.GetCurrentUser();
                InsertLog("View Edit Patient Form", user.Id, user.ClinicId, SD.AuditView);

                return View(patient);
            }
            catch
            {
                return View("Error");
            }
        }

        public IActionResult Archive(string id, string userId, string response)
        {
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    LogoutUser();
                    return RedirectToAction("Archive");
                }
                userId = AesOperation.DecryptString(userId, true);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                var clinicEmail = AesOperation.DecryptString(user.Clinic.EmailAddress);
                id = AesOperation.DecryptString(id, true);

                EmailSender emailSender = new EmailSender();

                if (response == "ArchiveApproved")
                {
                    // Send Approve Email to Clinic
                    string? urlParamId = AesOperation.EncryptString(userId, true);
                    string? callbackUrl = Url.Action("Index", "Patient", new { area = "Clinic", userid = urlParamId });

                    emailSender.SendEmailAsync(
                    clinicEmail,
                    "Archive Request Approved",
                    $"Your request has been approved by the patient and the record has been archived, please <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>click here</a> to return to the application.");

                    var obj = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == Int32.Parse(id));
                    if (obj == null)
                    {
                        return RedirectToAction("ProcessResponse", new { response = "ArchiveFailed" });
                    }

                    obj.IsArchived = true;
                    _unitOfWork.Patient.Update(obj);
                    _unitOfWork.Save();

                    InsertLog("Archive Patient", user.Id, user.ClinicId, SD.AuditArchive);

                    return View("ActionApproved");
                }
                else if (response == "ArchiveDenied")
                {
                    // Send Denied Email to Clinic
                    string? urlParamId = AesOperation.EncryptString(user.Id.ToString(), true);
                    string? callbackUrl = Url.Action("Index", "Patient", new { area = "Clinic", id = urlParamId });

                    emailSender.SendEmailAsync(
                    clinicEmail,
                    "Archive Request Denied",
                    $"Your request to archive has been denied by the patient.");


                    return View("ActionDenied");
                }
                else
                {
                    return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
                }
            }
            catch (DbUpdateException)
            {
                // Send email to clinic archiving failed

                return RedirectToAction("ProcessResponse", new { response = "ArchiveFailed" });
            }
            catch (Exception)
            {
                return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
            }
        }

        public IActionResult Delete(string id, string userId, string response)
        {
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    LogoutUser();
                    return RedirectToAction("Delete");
                }
                userId = AesOperation.DecryptString(userId, true);
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                user.Clinic = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
                var clinicEmail = AesOperation.DecryptString(user.Clinic.EmailAddress);
                id = AesOperation.DecryptString(id, true);

                EmailSender emailSender = new EmailSender();

                if (response == "DeleteApproved")
                {
                    // Send email to clinic
                    string? urlParamId = AesOperation.EncryptString(userId, true);
                    string? callbackUrl = Url.Action("Index", "Patient", new { area = "Clinic", userid = urlParamId });

                    emailSender.SendEmailAsync(
                    clinicEmail,
                    "Delete Request Approved",
                    $"Your request has been approved by the patient and the record has been Deleted, please <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>click here</a> to return to the application.");

                    var patient = _unitOfWork.Patient.GetFirstOrDefault(u => u.Id == Int32.Parse(id));
                    if (patient == null)
                    {
                        return RedirectToAction("ProcessResponse", new { response = "DeleteFailed" });
                    }

                    if (patient.ProfPicUrl != null)
                    {
                        string decryptedPic = AesOperation.DecryptString(patient.ProfPicUrl);
                        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, decryptedPic.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Add logic to delete all records at auditlogpatients
                    // because if we don't remove it delete will fail

                    var audits = _unitOfWork.AuditLogPatient.GetAll(x => x.Id == Int32.Parse(id));
                    _unitOfWork.AuditLogPatient.RemoveRange(audits);
                    _unitOfWork.Save();
                    _unitOfWork.Patient.Remove(patient);
                    _unitOfWork.Save();


                    InsertLog("Delete Patient", user.Id, user.ClinicId, SD.AuditDelete);

                    return View("ActionApproved");
                }
                else if (response == "DeleteDenied")
                {
                    // Send Denied Email to Clinic
                    string? urlParamId = AesOperation.EncryptString(user.Id.ToString(), true);
                    string? callbackUrl = Url.Action("Index", "Patient", new { area = "Clinic", id = urlParamId });

                    emailSender.SendEmailAsync(
                    clinicEmail,
                    "Delete Request Denied",
                    $"Your request to delete has been denied by the patient.");

                    return View("ActionDenied");
                }
                else
                {
                    return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
                }
            }
            catch (DbUpdateException)
            {
                // Send email to clinic delete failed

                return RedirectToAction("ProcessResponse", new { response = "DeleteFailed" });
            }
            catch (Exception)
            {
                return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> SendPortalRegistrationLink(Patient obj)
        {
            Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == obj.Id);

            if (patient.PortalAccess == true)
            {
                string? firstname = AesOperation.DecryptString(patient.FirstName);
                string? lastname = AesOperation.DecryptString(patient.LastName);
                TempData["error"] = $"Patient {firstname} {lastname} already has portal access";
                return View(nameof(IndexAsync));
            }

            if (obj.Email != patient.Email)
            {
                patient.Email = obj.Email;
                _unitOfWork.Patient.Update(patient);
                _unitOfWork.Save();
            }

            string? urlParamId = AesOperation.EncryptString(patient.Id.ToString(), true);
            string? callbackUrl = Url.Action("RegisterPatient", "Patient", new { area = "Clinic", id = urlParamId });

            EmailSender emailSender = new EmailSender();

            await emailSender.SendEmailAsync(
                obj.Email,
                "Patient Portal Registration",
                $"Proceed with your registration to the patient portal by <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>clicking here</a>.");

            TempData["success"] = $"Successfully sent email to {obj.Email}";

            var user = await _userService.GetCurrentUser();
            InsertLog("Send Patient Portal Registration Link", user.Id, user.ClinicId, SD.AuditEmail);

            return View(nameof(IndexAsync));
        }

        [HttpGet]
        public IActionResult RegisterPatient(string id)
        {
            if (id == null)
                return View("Error");

            try
            {
                
                int patientId = Int32.Parse(AesOperation.DecryptString(id, true));
                Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == patientId);

                if (patient.PortalAccess == true)
                {
                    InsertLog("Error Register Patient", patientId, SD.AuditView);
                    return RedirectToAction("PortalRegistrationError");
                }

                InsertLog("View Register Patient", patientId, SD.AuditView);

                RegisterPatientVM vm = new RegisterPatientVM()
                {
                    Id = id,
                    Name = AesOperation.DecryptString(patient.FirstName) + " " +
                    AesOperation.DecryptString(patient.MiddleName) + " " +
                    AesOperation.DecryptString(patient.LastName),
                    Email = patient.Email,
                };
                if (_signInManager.IsSignedIn(User))
                {
                    LogoutUser();
                    return RedirectToAction("RegisterPatient",vm);
                }
                return View("RegisterPatient", vm);
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(RegisterPatientVM obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int patientId = Int32.Parse(AesOperation.DecryptString(obj.Id, true));

                    Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == patientId);

                    if (obj.ConfirmDOB != AesOperation.DecryptString(patient.DOB))
                    {
                        RegisterPatientVM vm = new RegisterPatientVM()
                        {
                            Id = obj.Id,
                            Name = AesOperation.DecryptString(patient.FirstName) + " " +
                            AesOperation.DecryptString(patient.MiddleName) + " " +
                            AesOperation.DecryptString(patient.LastName),
                            Email = patient.Email,
                        };

                        ModelState.AddModelError("ConfirmDOB", "Your input for the date of birth does not match our record.");
                        return View(vm);
                    }

                    patient.Password = AesOperation.EncryptString(obj.Password);
                    patient.PortalAccess = true;

                    _unitOfWork.Patient.Update(patient);
                    _unitOfWork.Save();

                    string? callbackUrl = Url.Action("VerifyPatient", "Patient", new { area = "Clinic", id = obj.Id });

                    EmailSender emailSender = new EmailSender();

                    await emailSender.SendEmailAsync(
                        patient.Email,
                        "Account Verification",
                        $"Please complete your registration to the patient portal by <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>clicking here</a>.");

                    InsertLog("Register Patient Portal", patientId, SD.AuditUpdate);

                    return RedirectToAction("PortalRegistrationConfirmation");
                }
                catch
                {
                    return View("Error");
                }
            }
            return View(obj);
        }
        public IActionResult VerifyPatient(string id)
        {
            if (id == null)
                return View("Error");

            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    LogoutUser();
                    return RedirectToAction("VerifyPatient");
                }
                int patientId = Int32.Parse(AesOperation.DecryptString(id, true));

                Patient patient = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == patientId);

                if (patient == null)
                    return View("Error");

                if (patient.PortalAccess == false)
                    return View("Error");

                if (patient.IsVerified == true)
                {
                    InsertLog("Error Verify Patient", patientId, SD.AuditView);
                    return RedirectToAction("PatientVerificationError");
                }

                patient.IsVerified = true;

                _unitOfWork.Patient.Update(patient);
                _unitOfWork.Save();

                InsertLog("Verify Patient", patientId, SD.AuditUpdate);

                return RedirectToAction("PatientVerificationSuccess");
            }
            catch
            {
                return View("Error");
            }
        }
        public IActionResult PortalRegistrationError()
        {
            if (_signInManager.IsSignedIn(User))
            {
                LogoutUser();
                return RedirectToAction("PortalRegistrationError");
            }
            return View();
        }
        public IActionResult PortalRegistrationConfirmation()
        {
            if (_signInManager.IsSignedIn(User))
            {
                LogoutUser();
                return RedirectToAction("PortalRegistrationConfirmation");
            }
            return View();
        }
        public IActionResult PatientVerificationError()
        {
            if (_signInManager.IsSignedIn(User))
            {
                LogoutUser();
                return RedirectToAction("PatientVerificationError");
            }
            return View();
        }
        public IActionResult PatientVerificationSuccess()
        {
            if (_signInManager.IsSignedIn(User))
            {
                LogoutUser();
                return RedirectToAction("PatientVerificationSuccess");
            }

            return View();
        }

        #region API CALLS
        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<string> GetAllAsync(string status)
        {
            var user = await _userService.GetCurrentUser();

            IEnumerable<ClinicPatient> clinicPatients = _unitOfWork.ClinicPatient.GetAll(x => x.ClinicId == user.ClinicId);
            List<Patient> patientsFromDb = new List<Patient>();

            foreach (ClinicPatient obj in clinicPatients)
            {
                patientsFromDb.Add(_unitOfWork.Patient.GetFirstOrDefault(x => x.Id == obj.PatientId));
            }

            List<Patient> decryptedPatients = new List<Patient>();

            foreach (Patient obj in patientsFromDb)
            {
                decryptedPatients.Add(new Patient()
                {
                    Id = obj.Id,
                    FirstName = AesOperation.DecryptString(obj.FirstName),
                    LastName = AesOperation.DecryptString(obj.LastName),
                    DOB = AesOperation.DecryptString(obj.DOB),
                    Gender = AesOperation.DecryptString(obj.Gender),
                    DateAdded = obj.DateAdded,
                    LastModified = obj.LastModified,
                    IsArchived = obj.IsArchived,
                });
            }

            switch (status)
            {
                case "archived":
                    decryptedPatients = decryptedPatients.Where(x => x.IsArchived == true).ToList();
                    break;
                case "active":
                    decryptedPatients = decryptedPatients.Where(x => x.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }

            InsertLog("View Patient List", user.Id, user.ClinicId, SD.AuditView);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedPatients, jsonSettings);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> LoadAddFormAsync()
        {
            var user = await _userService.GetCurrentUser();
            InsertLog("View Add Patient Form", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_AddPatientForm");
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> GetPatientAsync(int id)
        {
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new Patient()
            {
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                DOB = AesOperation.DecryptString(objFromDb.DOB),
                Gender = AesOperation.DecryptString(objFromDb.Gender),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                Email = objFromDb.Email,
                HomeNumber = AesOperation.DecryptString(objFromDb.HomeNumber),
                MobileNumber = AesOperation.DecryptString(objFromDb.MobileNumber),
                ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
                PortalAccess = objFromDb.PortalAccess,
                IsVerified = objFromDb.IsVerified,
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Patient", user.Id, user.ClinicId, SD.AuditView);
            return PartialView("_ViewPatient", patient);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> LoadEditFormAsync(int id)
        {

            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new Patient()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                DOB = AesOperation.DecryptString(objFromDb.DOB),
                Email = objFromDb.Email,
                HomeNumber = AesOperation.DecryptString(objFromDb.HomeNumber),
                MobileNumber = AesOperation.DecryptString(objFromDb.MobileNumber),
                Gender = AesOperation.DecryptString(objFromDb.Gender),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Edit Patient Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditPatientForm", patient);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> LoadEditPatientRequestFormAsync(int id)
        {
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new Patient()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                Email = objFromDb.Email,
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Edit Patient Request Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditPatientRequestForm", patient);
        }

        public async Task<IActionResult> LoadArchivePatientRequestFormAsync(int id)
        {
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                Email = objFromDb.Email,
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Archive Patient Request Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ArchivePatientRequestForm", patient);
        }

        public async Task<IActionResult> LoadDeletePatientRequestFormAsync(int id)
        {
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                Email = objFromDb.Email,
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Delete Patient Request Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_DeletePatientRequestForm", patient);
        }

        [HttpGet]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_Dentist + "," + SD.Role_Assistant)]
        public async Task<IActionResult> LoadEmailLinkFormAsync(int id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

            Patient patient = new Patient()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                Email = objFromDb.Email,
            };

            var user = await _userService.GetCurrentUser();
            InsertLog("View Send Link Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EmailLinkForm", patient);
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

        public string? GetSubscriptionMessage(Subscription subscription)
        {
            if (subscription.Type == "Free") return null;

            DateTime today = DateTime.Now;

            DateTime dateToday = new(today.Year, today.Month, today.Day);
            DateTime billingDate = new(subscription.BillingDate.Year, subscription.BillingDate.Month, subscription.BillingDate.Day);

            if (billingDate.Month > today.Month) return null;

            int dateComparisonResult = DateTime.Compare(dateToday, billingDate);

            DateTime gracePeriod = subscription.BillingDate.AddDays(7);
            DateTime gracePeriodDate = new(gracePeriod.Year, gracePeriod.Month, gracePeriod.Day);


            if (dateComparisonResult == -1 && today.Day - billingDate.Day >= -7)
            {
                //Earlier
                return "Your next billing date is approaching! To pay your bill, go to your clinic account and click the 'Pay with GCash' button.";
            }
            else if (dateComparisonResult == 0)
            {
                //Today
                return $"The billing date is today! You have until {gracePeriod:dd MMMM yyyy} to pay before your account reverts to the FREE version. To pay your bill, go to your clinic account and click the 'Pay with GCash' button.";
            }
            else if (dateComparisonResult > 0)
            {
                //Later
                int gracePeriodComparisonResult = DateTime.Compare(dateToday, gracePeriodDate);

                if (gracePeriodComparisonResult == -1)
                {
                    //Earlier
                    return $"Your billing date has passed! You still have until {gracePeriod:dd MMMM yyyy} to pay before your account reverts to the FREE version. To pay your bill, go to your clinic account and click the 'Pay with GCash' button.";
                }
                else if (gracePeriodComparisonResult > 0)
                {
                    //Later

                    if (subscription.IsLockout == false)
                    {
                        subscription.IsLockout = true;

                        _unitOfWork.Subscription.Update(subscription);
                        _unitOfWork.Save();
                    }

                    return "Your account has been reverted to the FREE version. To pay your bill, go to your clinic account and click the 'Pay with GCash' button to reclaim your account's previous features and functionalities.";
                }
                else if (gracePeriodComparisonResult == 0)
                {
                    //Today
                    return $"The grace period expires today ({gracePeriod:dd MMMM yyyy}). If you do not pay today, your account will revert to the FREE version tomorrow. To pay your bill, go to your clinic account and click the 'Pay with GCash' button.";
                }
            }

            return null;
        }

        public void SetSubscriptionSession(Subscription subscription)
        {
            string? subscriptionType = HttpContext.Session.GetString(SD.SessionKeySubscriptionType);
            string? subscriptionIsLockout = HttpContext.Session.GetString(SD.SessionKeySubscriptionIsLockout);

            if (subscriptionType == null)
                HttpContext.Session.SetString(SD.SessionKeySubscriptionType, subscription.Type);

            if (subscriptionType != subscription.Type)
            {
                HttpContext.Session.Remove(SD.SessionKeySubscriptionType);
                HttpContext.Session.SetString(SD.SessionKeySubscriptionType, subscription.Type);
            }

            if (subscriptionIsLockout == null)
                HttpContext.Session.SetString(SD.SessionKeySubscriptionIsLockout, subscription.IsLockout ? "true" : "false");

            if (subscriptionIsLockout != (subscription.IsLockout ? "true" : "false"))
            {
                HttpContext.Session.Remove(SD.SessionKeySubscriptionIsLockout);
                HttpContext.Session.SetString(SD.SessionKeySubscriptionIsLockout, subscription.IsLockout ? "true" : "false");
            }
        }
        public async Task<IActionResult> LogoutUser()
        {
            await _signInManager.SignOutAsync();
           return RedirectToAction("Index","Clinic");
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