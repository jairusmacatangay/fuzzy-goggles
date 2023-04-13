using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VitasysEHR.DataAccess;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.PatientPortal.Controllers
{
    [Area("PatientPortal")]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(IUnitOfWork unitOfWork, 
            IWebHostEnvironment hostEnvironment,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));

            if (user != null)
            {
                Patient decryptedUser = new Patient()
                {
                    FirstName = AesOperation.DecryptString(user.FirstName),
                    LastName = AesOperation.DecryptString(user.LastName),
                    Email = user.Email,
                    Address = AesOperation.DecryptString(user.Address),
                    Province = AesOperation.DecryptString(user.Province),
                    ZipCode = AesOperation.DecryptString(user.ZipCode),
                    HomeNumber = AesOperation.DecryptString(user.HomeNumber),
                    MobileNumber = AesOperation.DecryptString(user.MobileNumber),
                    DOB = AesOperation.DecryptString(user.DOB),
                    Gender = AesOperation.DecryptString(user.Gender),
                    City = AesOperation.DecryptString(user.City),
                    ProfPicUrl = AesOperation.DecryptString(user.ProfPicUrl)
                };

                ViewData["CurrentPage"] = "accounts";

                InsertLog("View Profile", user.Id, SD.AuditView);

                return View(decryptedUser);
            }
            return Redirect("Account/Login");
        }

        [HttpGet]
        public  IActionResult Login()
        {
            int? id = HttpContext.Session.GetInt32(SD.SessionKeyPatientId);
            int? userId = HttpContext.Session.GetInt32(SD.SessionKeyUserId);
            if (userId != null) 
            {
                HttpContext.Session.Remove(SD.SessionKeyUserId);
                
            }else if (id != null)
            {
                return Redirect("Index");
            } 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Patient patient)
        {
            if (!ModelState.IsValid)
            {
                string email = Request.Form["Email"];
                string password = Request.Form["Password"];

                Patient user = _unitOfWork.Patient.GetFirstOrDefault(x => x.Email == email);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                    return View();
                }
                if(_signInManager.IsSignedIn(User))
                {
                    await _signInManager.SignOutAsync();
                    
                }
                if (user != null)
                {
                   
                    string? loginPass = AesOperation.DecryptString(user.Password);
                    
                    if (loginPass != password || user.Email != email)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                        return View();
                    }

                    if (!user.IsVerified)
                    {
                        ModelState.AddModelError(string.Empty, "Please check your email to verify your account.");
                        return View();
                    }
                    HttpContext.Session.SetInt32(SD.SessionKeyPatientId, user.Id);

                    string name = $"{AesOperation.DecryptString(user.FirstName)} {AesOperation.DecryptString(user.LastName)}";

                    HttpContext.Session.SetString(SD.SessionKeyPatientName, name);
                    HttpContext.Session.SetInt32(SD.SessionKeyPatientId, user.Id);

                    if (!string.IsNullOrEmpty(user.ProfPicUrl))
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, AesOperation.DecryptString(user.ProfPicUrl));
                    else
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, "/img/patients/prof-pic-placeholder.png");

                    InsertLog("Logged in to Patient Portal", user.Id, SD.AuditLogin);

                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }
            return View();
        }

        public IActionResult Logout()
        {
            InsertLog("Logged out of patient portal", HttpContext.Session.GetInt32(SD.SessionKeyPatientId), SD.AuditLogout);
            HttpContext.Session.Remove(SD.SessionKeyPatientId);
            return RedirectToAction("Index", "Home", new { area = "Clinic" });
        }

        [HttpGet]
        public IActionResult Update()
        {
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == HttpContext.Session.GetInt32(SD.SessionKeyPatientId));
            
            if (objFromDb == null)
            {
                return Redirect("Login");
            }

            Patient user = new Patient()
            {
                Id = objFromDb.Id,
                FirstName = AesOperation.DecryptString(objFromDb.FirstName),
                MiddleName = AesOperation.DecryptString(objFromDb.MiddleName),
                LastName = AesOperation.DecryptString(objFromDb.LastName),
                DOB = AesOperation.DecryptString(objFromDb.DOB),
                Email = objFromDb.Email,
                MobileNumber = AesOperation.DecryptString(objFromDb.MobileNumber),
                HomeNumber = AesOperation.DecryptString(objFromDb.HomeNumber),
                Gender = AesOperation.DecryptString(objFromDb.Gender),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                ProfPicUrl = AesOperation.DecryptString(objFromDb.ProfPicUrl),
            };

            ViewData["CurrentPage"] = "accounts";

            return View("Update", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Patient obj, IFormFile? file)
        {
            int? id = HttpContext.Session.GetInt32(SD.SessionKeyPatientId);
            Patient objFromDb = _unitOfWork.Patient.GetFirstOrDefault(x => x.Id == id);

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
                if(obj.Email != objFromDb.Email)
                    objFromDb.Email = obj.Email;
                if (obj.MobileNumber != AesOperation.DecryptString(objFromDb.MobileNumber))
                    objFromDb.MobileNumber = AesOperation.EncryptString(obj.MobileNumber);
                if (obj.HomeNumber != AesOperation.DecryptString(objFromDb.HomeNumber))
                    objFromDb.HomeNumber = AesOperation.EncryptString(obj.HomeNumber);
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
                    var upload = Path.Combine(wwwRootPath, @"img\patients");
                    var extension = Path.GetExtension(file.FileName);
                    var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!imageExtensions.Contains(extension))
                    {
                        TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                        return RedirectToAction("Index");
                    }
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

                    if (!string.IsNullOrEmpty(objFromDb.ProfPicUrl))
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, AesOperation.DecryptString(objFromDb.ProfPicUrl));
                    else
                        HttpContext.Session.SetString(SD.SessionKeyPatientProfPicUrl, "/img/patients/prof-pic-placeholder.png");
                }

                TempData["success"] = "Account updated successfully";

                _unitOfWork.Patient.Update(objFromDb);
                _unitOfWork.Save();

                InsertLog("Updated Patient Profile", id, SD.AuditUpdate);

                return RedirectToAction("Index");
            }

            return View(nameof(Index));
        }

        public void InsertLog(string activityType, int? patientId, string description)
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

    }
}
