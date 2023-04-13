using Microsoft.AspNetCore.Mvc;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using VitasysEHR.Utility;
using Newtonsoft.Json;
using VitasysEHR.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using VitasysEHRWeb.Utility;
using Microsoft.EntityFrameworkCore;

namespace VitasysEHRWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "View User Accounts List",
                ClinicId = user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                Description = SD.AuditView,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            return View();
        }

        [HttpGet]
        public IActionResult LoadEditRequestForm(string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            ApplicationUser user = new()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
            };

            return PartialView("_EditRequestForm", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequestAsync(string Id, string reason)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(reason)) 
                {
                    TempData["error"] = "Reason field cannot be empty";
                    return RedirectToAction("Index");
                }

                // Get user from db
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(g => g.Id == Id);
                if (user == null)
                {
                    // Display error message
                    TempData["error"] = "User does not exist!";
                    return RedirectToAction("Index");
                }

                // Create data to include as URL parameter
                ApplicationUser currentUser = GetCurrentUser();
                string? urlParamId = AesOperation.EncryptString(Id, true);
                string? urlParamEmail = AesOperation.EncryptString(currentUser.Email, true);

                // Create approve and deny urls
                string? approveUrl = Url.Action("ProcessResponse", "Account", new { area = "Admin", id = urlParamId,
                    email = urlParamEmail, response = "EditApproved" });
                string? denyUrl = Url.Action("ProcessResponse", "Account", new { area = "Admin", id = urlParamId, 
                    email = urlParamEmail, response = "EditDenied" });

                // Send email
                EmailSender emailSender = new();
                await emailSender.SendEmailAsync(
                    user.Email,
                    "VitaSys EHR Admin would like to request to edit your account",
                    "<div>The VitaSys EHR Admin is requesting to edit your account.</div><br />" +
                    "<div><strong>Reason:</strong></div>" + 
                    $"<div>{reason.Trim()}</div><br />" + 
                    "<div><strong>Approve Request:</strong></div>" + 
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{approveUrl}'>approve</a><br /><br />" +
                    "<div><strong>Deny Request:</strong></div>" + 
                    $"Click here to <a href='{Request.Scheme}://{Request.Host}{denyUrl}'>deny</a>");

                // Display success message
                TempData["success"] = "Edit user account request was successfully sent!";

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["error"] = "An unexpected error happened";
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessResponseAsync(string? id, string? email, string response)
        {
            try
            {
                // Decrypt data
                string? decryptedId = AesOperation.DecryptString(id, true);
                string? decryptedEmail = AesOperation.DecryptString(email, true);
                if (decryptedId == null || decryptedEmail == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                // Get user
                var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(g => g.Id == decryptedId);
                if (user == null)
                {
                    SetResponseMessage("UnexpectedError");
                    return View();
                }

                // EmailSender instance
                EmailSender emailSender = new();

                // Decrypt first and last name
                string? firstName = AesOperation.DecryptString(user.FirstName);
                string? lastName = AesOperation.DecryptString(user.LastName);

                if (response == "EditApproved")
                {
                    // Create edit url
                    string? editUrl = Url.Action("Update", "Account", new { area = "Admin", id });

                    // Send email to clinic
                    await emailSender.SendEmailAsync(
                        decryptedEmail,
                        $"User {firstName} {lastName} has approved your edit request",
                        $"<div>User <strong>{firstName} {lastName}</strong> has approved your edit request</div><br />" +
                        $"Click here to <a href='{Request.Scheme}://{Request.Host}{editUrl}'>edit</a> user's account<br /><br />");

                    // This will make the user's record editable
                    user.IsEditable = true;

                    // Update user record
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

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string? userid = claim.Value;
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userid);
        }

        [HttpGet]
        public IActionResult VerifyUser()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Update(string? id)
        {
            try
            {
                // Decrypt id
                string? userId = AesOperation.DecryptString(id, true);

                // Get user
                var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
                if (objFromDb == null)
                {
                    return View("Error");
                }

                // Redirect if IsEditable == false
                if (!objFromDb.IsEditable)
                {
                    TempData["error"] = "User's record cannot be edited";
                    return RedirectToAction("Index");
                }

                // Create new instance of ApplicationUser with decrypted values
                ApplicationUser user = new()
                {
                    Id = objFromDb.Id,
                    FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                    MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                    LastName = AesOperation.DecryptString(objFromDb.LastName),
                    DOB = AesOperation.DecryptString(objFromDb.DOB),
                    Email = objFromDb.Email,
                    PhoneNumber = AesOperation.DecryptString(objFromDb.PhoneNumber),
                    Gender = AesOperation.DecryptString(objFromDb.Gender),
                    Address = AesOperation.DecryptString(objFromDb.Address),
                    City = AesOperation.DecryptString(objFromDb.City),
                    Province = AesOperation.DecryptString(objFromDb.Province),
                    ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                    ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
                };

                return View(user);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ApplicationUser obj, IFormFile? file)
        {
            try
            {
                string? id = AesOperation.DecryptString(obj.Id, true);

                ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);
                if (objFromDb == null)
                {
                    return View("Error");
                }

                var errors = ModelState.Values.SelectMany(v => v.Errors);

                if (ModelState.IsValid)
                {
                    if (obj.FirstName != AesOperation.DecryptString(objFromDb.FirstName))
                        objFromDb.FirstName = AesOperation.EncryptString(obj.FirstName);
                    if (obj.MiddleName != AesOperation.DecryptString(objFromDb.MiddleName))
                        objFromDb.MiddleName = AesOperation.EncryptString(obj.MiddleName);
                    if (obj.LastName != AesOperation.DecryptString(objFromDb.LastName))
                        objFromDb.LastName = AesOperation.EncryptString(obj.LastName);
                    if (obj.DOB != AesOperation.DecryptString(objFromDb.DOB))
                        objFromDb.DOB = AesOperation.EncryptString(obj.DOB);
                    if (obj.PhoneNumber != AesOperation.DecryptString(objFromDb.PhoneNumber))
                        objFromDb.PhoneNumber = AesOperation.EncryptString(obj.PhoneNumber);
                    if (obj.Gender != AesOperation.DecryptString(objFromDb.Gender))
                        objFromDb.Gender = AesOperation.EncryptString(obj.Gender);
                    if (obj.Address != AesOperation.DecryptString(objFromDb.Address))
                        objFromDb.Address = AesOperation.EncryptString(obj.Address);
                    if (obj.City != AesOperation.DecryptString(objFromDb.City))
                        objFromDb.City = AesOperation.EncryptString(obj.City);
                    if (obj.Province != AesOperation.DecryptString(objFromDb.Province))
                        objFromDb.Province = AesOperation.EncryptString(obj.Province);
                    if (obj.ZipCode != AesOperation.DecryptString(objFromDb.ZipCode))
                        objFromDb.ZipCode = AesOperation.EncryptString(obj.ZipCode);

                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var upload = Path.Combine(wwwRootPath, @"img\user-profile-pics");
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

                        obj.ProfPicUrl = @"\img\user-profile-pics\" + fileName + extension;
                        objFromDb.ProfPicUrl = AesOperation.EncryptString(obj.ProfPicUrl);
                    }

                    objFromDb.IsEditable = false;
                    objFromDb.LastModified = DateTime.Now;

                    TempData["success"] = "Account updated successfully";

                    _unitOfWork.ApplicationUser.Update(objFromDb);

                    var user = GetCurrentUser();
                    InsertLog("Update User Account", user.Id, user.ClinicId, SD.AuditUpdate);

                    _unitOfWork.Save();

                    return RedirectToAction(nameof(Index));
                }

                return View("Index");
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

        [HttpPost]
        public IActionResult Approve(string id)
        {
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            user.AdminVerified = "Approved";

            _unitOfWork.ApplicationUser.Update(user);
            _unitOfWork.Save();

            TempData["success"] = "Successfully approved user account!";

            return View("VerifyUser");
        }

        [HttpPost]
        public IActionResult Deny(string id)
        {
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            user.AdminVerified = "Denied";

            _unitOfWork.ApplicationUser.Update(user);
            _unitOfWork.Save();

            TempData["success"] = "Successfully denied user account!";

            return View("VerifyUser");
        }

        [HttpGet]
        public string GetAll(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;
            var currentUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);

            var usersFromDb = _unitOfWork.ApplicationUser
                .GetAll()
                .Select(c => new AccountVM()
                {
                    ApplicationUser = c,
                }).ToList();

            List<AccountVM> decryptedUser = new List<AccountVM>();

            foreach (var obj in usersFromDb)
            {
                decryptedUser.Add(new AccountVM()
                {
                    ApplicationUser = new ApplicationUser()
                    {
                        Id = obj.ApplicationUser.Id,
                        FirstName = AesOperation.DecryptString(obj.ApplicationUser.FirstName),
                        LastName = AesOperation.DecryptString(obj.ApplicationUser.LastName),
                        DOB = AesOperation.DecryptString(obj.ApplicationUser.DOB),
                        Gender = AesOperation.DecryptString(obj.ApplicationUser.Gender),
                        DateAdded = obj.ApplicationUser.DateAdded,
                        LastModified = obj.ApplicationUser.LastModified
                    },
                });
            }

            switch (status)
            {
                case "archived":
                    decryptedUser = decryptedUser.Where(x => x.ApplicationUser.IsArchived == true).ToList();
                    break;
                case "active":
                    decryptedUser = decryptedUser.Where(x => x.ApplicationUser.IsArchived == false).ToList();
                    break;
                default:
                    break;
            }

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedUser, jsonSettings);
        }

        [HttpGet]
        public string GetAllUnverified(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;
            var currentUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);

            var usersFromDb = _unitOfWork.ApplicationUser
                .GetAll().Where(x=>x.AdminVerified != "Approved")
                .Select(c => new AccountVM()
                {
                    ApplicationUser = c,
                }).ToList();

            List<AccountVM> decryptedUser = new List<AccountVM>();

            foreach (var obj in usersFromDb)
            {
                decryptedUser.Add(new AccountVM()
                {
                    ApplicationUser = new ApplicationUser()
                    {
                        Id = obj.ApplicationUser.Id,
                        FirstName = AesOperation.DecryptString(obj.ApplicationUser.FirstName),
                        LastName = AesOperation.DecryptString(obj.ApplicationUser.LastName),
                        DOB = AesOperation.DecryptString(obj.ApplicationUser.DOB),
                        Gender = AesOperation.DecryptString(obj.ApplicationUser.Gender),
                        DateAdded = obj.ApplicationUser.DateAdded,
                        LastModified = obj.ApplicationUser.LastModified,
                        AdminVerified = obj.ApplicationUser.AdminVerified
                    },
                });
            }


            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyyy hh:mm tt";
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(decryptedUser, jsonSettings);
        }

        [HttpGet]
        public IActionResult GetUser(string id)
        {
            AccountVM accountVM = new();

            ApplicationUser obj = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            ApplicationUser user = new ApplicationUser()
            {
                Id = id,
                Email = obj.Email,
                FirstName = AesOperation.DecryptString(obj.FirstName),
                MiddleName = AesOperation.DecryptString(obj.MiddleName),
                LastName = AesOperation.DecryptString(obj.LastName),
                DOB = AesOperation.DecryptString(obj.DOB),
                Gender = AesOperation.DecryptString(obj.Gender),
                Address = AesOperation.DecryptString(obj.Address),
                PhoneNumber = AesOperation.DecryptString(obj.PhoneNumber),
                City = AesOperation.DecryptString(obj.City),
                Province = AesOperation.DecryptString(obj.Province),
                ZipCode = AesOperation.DecryptString(obj.ZipCode),
                ProfPicUrl = AesOperation.DecryptString(obj.ProfPicUrl)
            };

            accountVM.ApplicationUser = user;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            var _user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "View User Account",
                ClinicId = _user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = _user.Id,
                Description = SD.AuditView,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            return PartialView("_ViewAccount", accountVM);
        }

        [HttpGet]
        public IActionResult GetUnverifiedUser(string id)
        {
           

            AccountVM accountVM = new();

            ApplicationUser objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == id);

            ApplicationUser user = new ApplicationUser()
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
                ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
                PhoneNumber = AesOperation.DecryptString(objFromDb.PhoneNumber),
                AdminVerified = objFromDb.AdminVerified,
                Permit = AesOperation.DecryptString(objFromDb.Permit)
            };

            accountVM.ApplicationUser = user;

            return PartialView("_ViewUnverified", accountVM);
        }
    }
}