using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using VitasysEHR.DataAccess;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;
using VitasysEHRWeb.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner)]
    public class EmployeeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EmployeeController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _hostEnvironment = hostEnvironment;
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

            InsertLog("View Employee List", user.Id, user.ClinicId, SD.AuditView);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeVM obj, IFormFile? file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var currentUser = GetCurrentUser();

            ModelState.ClearValidationState("SelectedRoles");
            ModelState.MarkFieldValid("SelectedRoles");

            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = obj.ApplicationUser.Email,
                    Email = obj.ApplicationUser.Email,
                    FirstName = AesOperation.EncryptString(obj.ApplicationUser.FirstName),
                    MiddleName = AesOperation.EncryptString(obj.ApplicationUser.MiddleName),
                    LastName = AesOperation.EncryptString(obj.ApplicationUser.LastName),
                    DOB = AesOperation.EncryptString(obj.ApplicationUser.DOB),
                    Gender = AesOperation.EncryptString(obj.ApplicationUser.Gender),
                    Address = AesOperation.EncryptString(obj.ApplicationUser.Address),
                    City = AesOperation.EncryptString(obj.ApplicationUser.City),
                    Province = AesOperation.EncryptString(obj.ApplicationUser.Province),
                    ZipCode = AesOperation.EncryptString(obj.ApplicationUser.ZipCode)
                };

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(wwwRootPath, @"img\user-profile-pics");
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!imageExtensions.Contains(extension))
                    {
                        //ModelState.AddModelError("DocumentUrl", "The file type is not supported in this field. Please upload images only.");
                        TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }

                        obj.ApplicationUser.ProfPicUrl = @"\img\user-profile-pics\" + fileName + extension;
                        applicationUser.ProfPicUrl = AesOperation.EncryptString(obj.ApplicationUser.ProfPicUrl);
                    }

                }

                // assign created user to the same clinic as the logged in user
                applicationUser.ClinicId = currentUser.ClinicId;

                IdentityResult result = await _userManager.CreateAsync(applicationUser, obj.Password);

                if (result.Succeeded)
                {
                    obj.SelectedRoles = new() { obj.Role };
                    await _userManager.AddToRolesAsync(applicationUser, obj.SelectedRoles);

                    //Send email here
                    var userId = await _userManager.GetUserIdAsync(applicationUser);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code },
                        protocol: Request.Scheme);

                    EmailSender emailSender = new EmailSender();

                    await emailSender.SendEmailAsync(applicationUser.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br/><br/> " +
                        $"Your Login Credentials are <br/>Username: {applicationUser.Email} <br/>Password: {obj.Password} <br/><br/>" +
                        $"<b>*Note: It is STRONGLY recommended to change your password once logged in.</b>");


                    TempData["success"] = "Employee created successfully";
                    InsertLog("Create Employee", currentUser.Id, currentUser.ClinicId, SD.AuditCreate);
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        TempData["error"] = "Employee not created: " + error.Description;
                }
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAsync(string id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            try
            {
                //decrypt id
                string? userId = AesOperation.DecryptString(id, true);

                ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                if (objFromDb == null)
                {
                    return View("Error");
                }

                // Redirect if IsEditable == false
                if (!objFromDb.IsEditable)
                {
                    TempData["error"] = "User's employee record cannot be edited";
                    return RedirectToAction("Index");
                }

                EmployeeVM vm = new()
                {
                    ApplicationUser = new()
                    {
                        Id = userId,
                        FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                        MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                        LastName = AesOperation.DecryptString(objFromDb.LastName),
                        DOB = AesOperation.DecryptString(objFromDb.DOB),
                        Gender = AesOperation.DecryptString(objFromDb.Gender),
                        Address = AesOperation.DecryptString(objFromDb.Address),
                        City = AesOperation.DecryptString(objFromDb.City),
                        Province = AesOperation.DecryptString(objFromDb.Province),
                        ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                        ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl)
                    },
                    RoleList = new(),
                };

                var selectedRoles = await _userManager.GetRolesAsync(vm.ApplicationUser);
                vm.Role = selectedRoles[0];

                foreach (var item in _roleManager.Roles)
                {
                    if (item.Name == "Admin") continue;

                    vm.RoleList.Add(new SelectListItem
                    {
                        Text = item.Name,
                        Value = item.Name,
                        Selected = selectedRoles.Contains(item.Name)
                    });
                }

                ApplicationUser user = GetCurrentUser();
                InsertLog("View Edit Employee Form", user.Id, user.ClinicId, SD.AuditView);
                return View(vm);
            }
            catch(Exception)
            {
                return View("Error");
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string id, EmployeeVM obj, IFormFile? file)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                string wwwRootPath = _hostEnvironment.WebRootPath;
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                ModelState.ClearValidationState("Password");
                ModelState.MarkFieldValid("Password");
                ModelState.ClearValidationState("ConfirmPassword");
                ModelState.MarkFieldValid("ConfirmPassword");
                ModelState.ClearValidationState("SelectedRoles");
                ModelState.MarkFieldValid("SelectedRoles");

                if (ModelState.IsValid)
                {
                    // Update only modified fields
                    if (obj.ApplicationUser.FirstName != AesOperation.DecryptString(user.FirstName))
                        user.FirstName = AesOperation.EncryptString(obj.ApplicationUser.FirstName);
                    if (obj.ApplicationUser.MiddleName != AesOperation.DecryptString(user.MiddleName))
                        user.MiddleName = AesOperation.EncryptString(obj.ApplicationUser.MiddleName);
                    if (obj.ApplicationUser.LastName != AesOperation.DecryptString(user.LastName))
                        user.LastName = AesOperation.EncryptString(obj.ApplicationUser.LastName);
                    if (obj.ApplicationUser.DOB != AesOperation.DecryptString(user.DOB))
                        user.DOB = AesOperation.EncryptString(obj.ApplicationUser.DOB);
                    if (obj.ApplicationUser.Gender != AesOperation.DecryptString(user.Gender))
                        user.Gender = AesOperation.EncryptString(obj.ApplicationUser.Gender);
                    if (obj.ApplicationUser.Address != AesOperation.DecryptString(user.Address))
                        user.Address = AesOperation.EncryptString(obj.ApplicationUser.Address);
                    if (obj.ApplicationUser.City != AesOperation.DecryptString(user.City))
                        user.City = AesOperation.EncryptString(obj.ApplicationUser.City);
                    if (obj.ApplicationUser.Province != AesOperation.DecryptString(user.Province))
                        user.Province = AesOperation.EncryptString(obj.ApplicationUser.Province);
                    if (obj.ApplicationUser.ZipCode != AesOperation.DecryptString(user.ZipCode))
                        user.ZipCode = AesOperation.EncryptString(obj.ApplicationUser.ZipCode);

                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var upload = Path.Combine(wwwRootPath, @"img\user-profile-pics");
                        var extension = Path.GetExtension(file.FileName).ToLower();


                        if (!imageExtensions.Contains(extension))
                        {
                            TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            if (obj.ApplicationUser.ProfPicUrl != null)
                            {
                                var oldImagePath = Path.Combine(wwwRootPath, obj.ApplicationUser.ProfPicUrl.TrimStart('\\'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                            {
                                file.CopyTo(fileStreams);
                            }

                            obj.ApplicationUser.ProfPicUrl = @"\img\user-profile-pics\" + fileName + extension;
                            user.ProfPicUrl = AesOperation.EncryptString(obj.ApplicationUser.ProfPicUrl);
                        }

                    }

                    if (user != null)
                    {
                        user.IsEditable = false;
                        user.LastModified = DateTime.Now;
                        _unitOfWork.ApplicationUser.Update(user);
                        _unitOfWork.Save();

                        var roles = await _userManager.GetRolesAsync(user);
                        var removeRoles = await _userManager.RemoveFromRolesAsync(user, roles);

                        if (removeRoles.Succeeded)
                        {
                            obj.SelectedRoles = new() { obj.Role };
                            var addRoles = await _userManager.AddToRolesAsync(user, obj.SelectedRoles);
                            if (addRoles.Succeeded)
                            {
                                ApplicationUser currentUser = GetCurrentUser();
                                InsertLog("Update Employee", currentUser.Id, currentUser.ClinicId, SD.AuditUpdate);
                                TempData["success"] = "Employee updated successfully";
                            }
                            else
                            {
                                foreach (IdentityError error in removeRoles.Errors)
                                {
                                    TempData["error"] = "Employee not updated: " + error.Description;
                                }
                            }
                        }
                    }

                    return RedirectToAction("Index");
                }

                var selectedRoles = await _userManager.GetRolesAsync(obj.ApplicationUser);
                obj.RoleList = new();
                foreach (var item in _roleManager.Roles)
                {
                    if (item.Name == "Admin") continue;

                    obj.RoleList.Add(new SelectListItem
                    {
                        Text = item.Name,
                        Value = item.Name,
                        Selected = selectedRoles.Contains(item.Name)
                    });
                }

                return View(obj);
            }
            catch (DbUpdateException)
            {
                TempData["error"] = "Update failed. Please try again later.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View("Error");
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequest(string Id, string reason)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(reason))
                {
                    TempData["error"] = "Reason field cannot be empty";
                    return RedirectToAction("Index");
                }

                //Get user from DB
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == Id, includeProperties:"Clinic");
                if(user == null)
                {
                    TempData["error"] = "User does not exist!";
                    return RedirectToAction("Index");
                }

                //Create data to include as URL parameter
                ApplicationUser currentUser = GetCurrentUser();
                string? urlParamId = AesOperation.EncryptString(Id, true);
                string? urlParamEmail = AesOperation.EncryptString(currentUser.Email, true);

                //Create approve and deny urls
                string? approveUrl = Url.Action("ProcessResponse", "Employee", new {area = "Clinic", id = urlParamId, email = urlParamEmail, response = "EditApproved"});
                string? denyUrl = Url.Action("ProcessResponse", "Employee", new {area = "Clinic", id = urlParamId, email = urlParamEmail, response = "EditDenied"});

                //Send email
                EmailSender emailSender = new();
                await emailSender.SendEmailAsync(
                     user.Email,
                     $"{AesOperation.DecryptString(user.Clinic.Name)} would like to request to edit your account",
                     $"<div>{AesOperation.DecryptString(user.Clinic.Name)} is requesting to edit your account.</div><br />" +
                     "<div><strong>Reason:</strong></div>" +
                     $"<div>{reason.Trim()}</div><br />" +
                     "<div><strong>Approve Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveUrl}'>approve</a><br /><br />" +
                     "<div><strong>Deny Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyUrl}'>deny</a>");

                //Display success message
                TempData["success"] = "Edit employee request was successfully sent!";

                InsertLog("Edit Employee Request Email", currentUser.Id, currentUser.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["error"] = "An unexpected error happened";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveRequest(string Id, string reason)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(reason))
                {
                    TempData["error"] = "Reason field cannot be empty";
                    return RedirectToAction("Index");
                }

                //Get user from DB
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == Id, includeProperties: "Clinic");
                if (user == null)
                {
                    TempData["error"] = "User does not exist!";
                    return RedirectToAction("Index");
                }

                //Create data to include as URL parameter
                ApplicationUser currentUser = GetCurrentUser();
                string? urlParamId = AesOperation.EncryptString(Id, true);
                string? urlParamEmail = AesOperation.EncryptString(currentUser.Email, true);

                //Create approve and deny urls
                string? approveUrl = Url.Action("Archive", "Employee", new { area = "Clinic", id = urlParamId, email = urlParamEmail, response = "ArchiveApproved" });
                string? denyUrl = Url.Action("Archive", "Employee", new { area = "Clinic", id = urlParamId, email = urlParamEmail, response = "ArchiveDenied" });

                // Send email
                EmailSender emailSender = new();
                await emailSender.SendEmailAsync(
                     user.Email,
                     $"{AesOperation.DecryptString(user.Clinic.Name)} would like to request to archive your account",
                     $"<div>{AesOperation.DecryptString(user.Clinic.Name)} is requesting to archive your account.</div><br />" +
                     "<div><strong>Reason:</strong></div>" +
                     $"<div>{reason.Trim()}</div><br />" +
                     "<div><strong>Approve Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveUrl}'>approve</a><br /><br />" +
                     "<div><strong>Deny Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyUrl}'>deny</a>");

                TempData["success"] = "Archive employee request was successfully sent!";

                InsertLog("Archive Employee Request Email", currentUser.Id, currentUser.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequest(string Id, string reason)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(reason))
                {
                    TempData["error"] = "Reason field cannot be empty";
                    return RedirectToAction("Index");
                }

                //Get user from DB
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == Id, includeProperties: "Clinic");
                if (user == null)
                {
                    TempData["error"] = "User does not exist!";
                    return RedirectToAction("Index");
                }

                //Create data to include as URL parameter
                ApplicationUser currentUser = GetCurrentUser();
                string? urlParamId = AesOperation.EncryptString(Id, true);
                string? urlParamEmail = AesOperation.EncryptString(currentUser.Email, true);

                //Create approve and deny urls
                string? approveUrl = Url.Action("Delete", "Employee", new { area = "Clinic", id = urlParamId, email = urlParamEmail, response = "DeleteApproved" });
                string? denyUrl = Url.Action("Delete", "Employee", new { area = "Clinic", id = urlParamId, email = urlParamEmail, response = "DeleteDenied" });

                // Send email
                EmailSender emailSender = new();
                await emailSender.SendEmailAsync(
                     user.Email,
                     $"{AesOperation.DecryptString(user.Clinic.Name)} would like to request to delete your account",
                     $"<div>{AesOperation.DecryptString(user.Clinic.Name)} is requesting to delete your account.</div><br />" +
                     "<div><strong>Reason:</strong></div>" +
                     $"<div>{reason.Trim()}</div><br />" +
                     "<div><strong>Approve Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveUrl}'>approve</a><br /><br />" +
                     "<div><strong>Deny Request:</strong></div>" +
                     $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyUrl}'>deny</a>");

                TempData["success"] = "Delete employee request was successfully sent!";

                InsertLog("Delete Employee Request Email", currentUser.Id, currentUser.ClinicId, SD.AuditEmail);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessResponseAsync(string? id, string? email, string response)
        {
            try
            {
                //Decrypt 
                string? decryptedId = AesOperation.DecryptString(id, true);
                string? decryptedEmail = AesOperation.DecryptString(email, true);
                if (decryptedId == null || decryptedEmail == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                //Get user
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == decryptedId);
                if (user == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                //EmailSender
                EmailSender emailSender = new();

                //Decrypt first name and last name
                string? firstName = AesOperation.DecryptString(user.FirstName);
                string? lastName = AesOperation.DecryptString(user.LastName);

                if (response == "EditApproved")
                {
                    string? editUrl = Url.Action("Update", "Employee", new { area = "Clinic", id });
                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has approved your edit request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has approved your edit request</div><br />" +
                        $"Click here to <a href='{Request.Scheme}://{Request.Host}{editUrl}'>edit</a> user's employee account<br /><br />");

                    user.IsEditable = true;

                    _unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();
                }
                else if (response == "EditDenied")
                {
                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has denied your edit request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has denied your edit request." +
                        "You won't be able to edit the user's employee record.</div>");
                }
                else
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                SetResponseMessage(response);
                return View();
            }
            catch
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
        public string GetAll(string status)
        {
            if (AuthorizeAccess() == false) return String.Empty;

            var currentUser = GetCurrentUser();

            var usersFromDb = _unitOfWork.ApplicationUser
                .GetAll()
                .Where(x => x.ClinicId == currentUser.ClinicId)
                .Select(c => new EmployeeVM()
                {
                    ApplicationUser = c,
                    SelectedRoles = _userManager.GetRolesAsync(c).Result.ToList()
                }).ToList();

            List<EmployeeVM> decryptedEmp = new List<EmployeeVM>();

            foreach (var obj in usersFromDb)
            {
                decryptedEmp.Add(new EmployeeVM()
                {
                    ApplicationUser = new ApplicationUser()
                    {
                        Id = obj.ApplicationUser.Id,
                        FirstName = AesOperation.DecryptString(obj.ApplicationUser.FirstName),
                        LastName = AesOperation.DecryptString(obj.ApplicationUser.LastName),
                        DateAdded = obj.ApplicationUser.DateAdded,
                        LastModified = obj.ApplicationUser.LastModified,
                        IsArchived = obj.ApplicationUser.IsArchived,
                    },
                    SelectedRoles = obj.SelectedRoles
                });
            }

            switch (status)
            {
                case "archived":
                    decryptedEmp = decryptedEmp.Where(x => x.ApplicationUser.IsArchived == true).ToList();
                    break;
                case "active":
                    decryptedEmp = decryptedEmp.Where(x => x.ApplicationUser.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedEmp, jsonSettings);
        }

        [HttpGet]
        public IActionResult LoadAddForm()
        {
            if (AuthorizeAccess() == false) return View("Error");

            EmployeeVM employeeVM = new EmployeeVM()
            {
                ApplicationUser = new ApplicationUser(),
                RoleList = _roleManager.Roles.Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Name
                }).Where(x => x.Text != "Admin").ToList()

            };

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Create Employee Form", user.Id, user.ClinicId, SD.AuditCreate);

            return PartialView("_AddEmployeeForm", employeeVM);
        }

        [HttpGet]
        public IActionResult GetEmployee(string id)
        {
            if (AuthorizeAccess() == false) return View("Error");

            EmployeeVM employeeVM = new();

            ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            ApplicationUser employee = new ApplicationUser()
            {
                Id = id,
                Email = objFromDb.Email,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                DOB = AesOperation.DecryptString(objFromDb.DOB),
                Gender = AesOperation.DecryptString(objFromDb.Gender),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl)
            };

            employeeVM.ApplicationUser = employee;
            employeeVM.SelectedRoles = _userManager.GetRolesAsync(employeeVM.ApplicationUser).Result.ToList();

            ApplicationUser user = GetCurrentUser();
            InsertLog("View Employee", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ViewEmployee", employeeVM);
        }

        [HttpGet]
        public IActionResult LoadEditRequestForm(string id)
        {
            ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);
            if (objFromDb == null)
            {
                TempData["error"] = "Employee does not exist.";
                return RedirectToAction("Index");
            }

            ApplicationUser employee = new()
            {
                Id = id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
            };

            var user = GetCurrentUser();
            InsertLog("View Edit Employee Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditEmployeeRequestForm", employee);
        }

        [HttpGet]
        public IActionResult LoadArchiveEmployeeRequestForm(string id)
        {
            ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);
            if (objFromDb == null)
            {
                TempData["error"] = "Employee does not exist.";
                return RedirectToAction("Index");
            }

            ApplicationUser employee = new()
            {
                Id = id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
            };

            var user = GetCurrentUser();
            InsertLog("View Archive Employee Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_ArchiveEmployeeRequestForm", employee);
        }

        [HttpGet]
        public IActionResult LoadDeleteEmployeeRequestForm(string id)
        {
            ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);
            if (objFromDb == null)
            {
                TempData["error"] = "Employee does not exist.";
                return RedirectToAction("Index");
            }

            ApplicationUser employee = new()
            {
                Id = id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
            };

            var user = GetCurrentUser();
            InsertLog("View Delete Employee Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_DeleteEmployeeRequestForm", employee);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ArchiveAsync(string? id, string? email, string response)
        {
            try
            {
                //Decrypt 
                string? decryptedId = AesOperation.DecryptString(id, true);
                string? decryptedEmail = AesOperation.DecryptString(email, true);
                if (decryptedId == null || decryptedEmail == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                //Get user
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == decryptedId);
                if (user == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                //EmailSender
                EmailSender emailSender = new();

                //Decrypt first name and last name
                string? firstName = AesOperation.DecryptString(user.FirstName);
                string? lastName = AesOperation.DecryptString(user.LastName);

                if (user.IsArchived)
                {
                    return View("ActionArchived");
                }

                if (response == "ArchiveApproved")
                {
                    string? callbackUrl = Url.Action("Index", "Employee", new { area = "Clinic", id });
                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has approved your archive request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has approved your archive request</div><br />" +
                        $"Click <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>here</a> to return to the application.<br /><br />");

                    user.IsArchived = true;
                    user.LastModified = DateTime.Now;

                    _unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();

                    ApplicationUser currentUser = GetCurrentUser();
                    InsertLog("Archive Employee", currentUser.Id, currentUser.ClinicId, SD.AuditArchive);

                    return View("ActionApproved");
                }
                else if (response == "ArchiveDenied")
                {
                    // Send Denied Email to Clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has denied your archive request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has denied your archive request." +
                        "You won't be able to archive the user's employee record.</div>");

                    return View("ActionDenied");
                }
                else
                {
                    return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
                }
            }
            catch (DbUpdateException)
            {
                TempData["error"] = "Update failed. Please try again later.";
                return RedirectToAction("ProcessResponse", new { response = "ArchiveFailed" });
            }
            catch (Exception)
            {
                return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> DeleteAsync(string? id, string? email, string response)
        {
            try
            {
                //Decrypt 
                string? decryptedId = AesOperation.DecryptString(id, true);
                string? decryptedEmail = AesOperation.DecryptString(email, true);
                if (decryptedId == null || decryptedEmail == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                //Get user
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == decryptedId);
                if (user == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View("ActionDeleted");
                }

                //EmailSender
                EmailSender emailSender = new();

                //Decrypt first name and last name
                string? firstName = AesOperation.DecryptString(user.FirstName);
                string? lastName = AesOperation.DecryptString(user.LastName);

                if (response == "DeleteApproved")
                {
                    string? callbackUrl = Url.Action("Index", "Employee", new { area = "Clinic", id });
                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has approved your delete request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has approved your delete request</div><br />" +
                        $"Click <a href='{Request.Scheme}://{Request.Host}{callbackUrl}'>here</a> to return to the application.<br /><br />");

                    if (user.ProfPicUrl != null)
                    {
                        string decryptedPic = AesOperation.DecryptString(user.ProfPicUrl);
                        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, decryptedPic.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Add logic to delete all employee's audit log records
                    var audits = _unitOfWork.AuditLog.GetAll().Where(x => x.UserId == user.Id);
                    _unitOfWork.AuditLog.RemoveRange(audits);
                    _unitOfWork.Save();
                    _unitOfWork.ApplicationUser.Remove(user);
                    _unitOfWork.Save();

                    ApplicationUser currentUser = GetCurrentUser();
                    InsertLog("Delete Employee", currentUser.Id, currentUser.ClinicId, SD.AuditDelete);

                    return View("ActionApproved");
                }
                else if (response == "DeleteDenied")
                {
                    // Send Denied Email to Clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has denied your delete request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has denied your delete request." +
                        "You won't be able to delete the user's employee record.</div>");

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
                TempData["error"] = "Update failed. Please try again later.";
                return RedirectToAction("ProcessResponse", new { response = "DeleteFailed" });
            }
            catch (Exception)
            {
                return RedirectToAction("ProcessResponse", new { response = "UnexpectedError" });
            }
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
