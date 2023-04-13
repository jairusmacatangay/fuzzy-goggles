using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Models.ViewModels;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Clinic.Controllers
{
    [Area("Clinic")]
    [Authorize(Roles = SD.Role_Owner)]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            IUnitOfWork unitOfWork, 
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = GetCurrentUser();

            if (user.Id == null) return View("Error");
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

            var objFromDb = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);
            if (objFromDb == null) return View("Error");

            VitasysEHR.Models.Clinic clinic = new VitasysEHR.Models.Clinic()
            {
                LogoUrl = AesOperation.DecryptString(objFromDb.LogoUrl),
                Name = AesOperation.DecryptString(objFromDb.Name),
                EmailAddress = AesOperation.DecryptString(objFromDb.EmailAddress),
                OfficePhone = AesOperation.DecryptString(objFromDb.OfficePhone),
                MobilePhone = AesOperation.DecryptString(objFromDb.MobilePhone),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                StartTime = objFromDb.StartTime,
                EndTime = objFromDb.EndTime
            };

            var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

            if (subscription == null) return RedirectToAction("Index", "Subscription", new { area = "Clinic" });

            var vm = new ClinicAccountVM();
            vm.Clinic = clinic;
            vm.Subscription = subscription;
            vm.BillingDate = subscription.BillingDate.ToString("dd MMMM yyyy");
            vm.IsDueForPayment = SetIsDueForPayment(subscription);

            string? message = GetSubscriptionMessage(subscription);

            if (message != null)
            {
                TempData["SubscriptionAlertMessage"] = message;
            }

            SetSubscriptionSession(subscription);

            InsertLog("View Clinic Account", user.Id, user.ClinicId, SD.AuditView);

            return View(vm);
        }

        public IActionResult CreateClinic()
        {
            ApplicationUser user = GetCurrentUser();
            if(user.ClinicId != null)
            {
                return RedirectToAction("Index", "Account");
            }

            ClinicVM clinic = new ClinicVM();

            clinic.StartTimeList = new List<SelectListItem>
            {
                new SelectListItem { Text = "8AM", Value = "0-8AM"},
                new SelectListItem { Text = "9AM", Value = "1-9AM"},
                new SelectListItem { Text = "10AM", Value = "2-10AM"},
                new SelectListItem { Text = "11AM", Value = "3-11AM"},
                new SelectListItem { Text = "12NN", Value = "4-12NN"},
                new SelectListItem { Text = "1PM", Value = "5-1PM"},
                new SelectListItem { Text = "2PM", Value = "6-2PM"},
                new SelectListItem { Text = "3PM", Value = "7-3PM"},
                new SelectListItem { Text = "4PM", Value = "8-4PM"},
                new SelectListItem { Text = "5PM", Value = "9-5PM"},
                new SelectListItem { Text = "6PM", Value = "10-6PM"},
                new SelectListItem { Text = "7PM", Value = "11-7PM"},
            };

            clinic.EndTimeList = new List<SelectListItem>
            {
                new SelectListItem { Text = "8AM", Value = "0-8AM"},
                new SelectListItem { Text = "9AM", Value = "1-9AM"},
                new SelectListItem { Text = "10AM", Value = "2-10AM"},
                new SelectListItem { Text = "11AM", Value = "3-11AM"},
                new SelectListItem { Text = "12NN", Value = "4-12NN"},
                new SelectListItem { Text = "1PM", Value = "5-1PM"},
                new SelectListItem { Text = "2PM", Value = "6-2PM"},
                new SelectListItem { Text = "3PM", Value = "7-3PM"},
                new SelectListItem { Text = "4PM", Value = "8-4PM"},
                new SelectListItem { Text = "5PM", Value = "9-5PM"},
                new SelectListItem { Text = "6PM", Value = "10-6PM"},
                new SelectListItem { Text = "7PM", Value = "11-7PM"},
            };
            InsertLog("View Create Clinic Form", user.Id, user.ClinicId, SD.AuditView);
            return View(clinic);
        }

        [HttpPost]
        public IActionResult CreateClinic(ClinicVM obj, IFormFile? file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            ApplicationUser user = GetCurrentUser();

            if (ModelState.IsValid)
            {
                var splitStartTime = obj.Clinic.StartTime.Split("-");
                var startTimeIndex = splitStartTime[0];

                var splitEndTime = obj.Clinic.EndTime.Split("-");
                var endTimeIndex = splitEndTime[0];

                if (int.Parse(endTimeIndex) < int.Parse(startTimeIndex) || int.Parse(endTimeIndex) == int.Parse(startTimeIndex))
                {
                    obj.StartTimeList = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "8AM", Value = "0-8AM"},
                        new SelectListItem { Text = "9AM", Value = "1-9AM"},
                        new SelectListItem { Text = "10AM", Value = "2-10AM"},
                        new SelectListItem { Text = "11AM", Value = "3-11AM"},
                        new SelectListItem { Text = "12NN", Value = "4-12NN"},
                        new SelectListItem { Text = "1PM", Value = "5-1PM"},
                        new SelectListItem { Text = "2PM", Value = "6-2PM"},
                        new SelectListItem { Text = "3PM", Value = "7-3PM"},
                        new SelectListItem { Text = "4PM", Value = "8-4PM"},
                        new SelectListItem { Text = "5PM", Value = "9-5PM"},
                        new SelectListItem { Text = "6PM", Value = "10-6PM"},
                        new SelectListItem { Text = "7PM", Value = "11-7PM"},
                    };

                    obj.EndTimeList = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "8AM", Value = "0-8AM"},
                        new SelectListItem { Text = "9AM", Value = "1-9AM"},
                        new SelectListItem { Text = "10AM", Value = "2-10AM"},
                        new SelectListItem { Text = "11AM", Value = "3-11AM"},
                        new SelectListItem { Text = "12NN", Value = "4-12NN"},
                        new SelectListItem { Text = "1PM", Value = "5-1PM"},
                        new SelectListItem { Text = "2PM", Value = "6-2PM"},
                        new SelectListItem { Text = "3PM", Value = "7-3PM"},
                        new SelectListItem { Text = "4PM", Value = "8-4PM"},
                        new SelectListItem { Text = "5PM", Value = "9-5PM"},
                        new SelectListItem { Text = "6PM", Value = "10-6PM"},
                        new SelectListItem { Text = "7PM", Value = "11-7PM"},
                    };
                    TempData["error"] = "End Time must not be the same or earlier than Start Time";
                    return View("CreateClinic", obj);
                }

                VitasysEHR.Models.Clinic clinic = new VitasysEHR.Models.Clinic()
                {
                    Name = AesOperation.EncryptString(obj.Clinic.Name),
                    Address = AesOperation.EncryptString(obj.Clinic.Address),
                    City = AesOperation.EncryptString(obj.Clinic.City),
                    Province = AesOperation.EncryptString(obj.Clinic.Province),
                    ZipCode = AesOperation.EncryptString(obj.Clinic.ZipCode),
                    OfficePhone = AesOperation.EncryptString(obj.Clinic.OfficePhone),
                    MobilePhone = AesOperation.EncryptString(obj.Clinic.MobilePhone),
                    EmailAddress = AesOperation.EncryptString(obj.Clinic.EmailAddress),
                    StartTime = obj.Clinic.StartTime,
                    EndTime = obj.Clinic.EndTime
                };

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(wwwRootPath, @"img\clinic-logos");
                    var extension = Path.GetExtension(file.FileName);
                    if (!imageExtensions.Contains(extension))
                    {
                        TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                        return Redirect("CreateClinic");
                    }
                    else
                    {
                        using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.Clinic.LogoUrl = @"\img\clinic-logos\" + fileName + extension;
                        clinic.LogoUrl = AesOperation.EncryptString(obj.Clinic.LogoUrl);
                    }
                }

                _unitOfWork.Clinic.Add(clinic);
                _unitOfWork.Save();

                user.ClinicId = clinic.Id;
                user.Clinic = clinic;
                _unitOfWork.ApplicationUser.Update(user);
                _unitOfWork.Save();

                InsertLog("Create Clinic Profile", user.Id, user.ClinicId, SD.AuditCreate);

                TempData["success"] = "Clinic profile created successfully!";

                return RedirectToAction("Index", "Subscription");
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateLogo(VitasysEHR.Models.Clinic obj, IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(wwwRootPath, @"img\clinic-logos");
                var extension = Path.GetExtension(file.FileName);
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (obj.LogoUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.LogoUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
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
                    string logoUrl = @"\img\clinic-logos\" + fileName + extension;
                    obj.LogoUrl = AesOperation.EncryptString(logoUrl);
                }
            }

            TempData["success"] = "Clinic logo updated successfully";

            _unitOfWork.Clinic.UpdateLogo(obj);
            _unitOfWork.Save();

            ApplicationUser user = GetCurrentUser();
            InsertLog("Update Clinic Logo", user.Id, user.ClinicId, SD.AuditUpdate);

            return RedirectToAction("Index"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAccount(ClinicVM obj)
        {
            if (ModelState.IsValid)
            {
                var splitStartTime = obj.Clinic.StartTime.Split("-");
                var startTimeIndex = splitStartTime[0];

                var splitEndTime = obj.Clinic.EndTime.Split("-");
                var endTimeIndex = splitEndTime[0];

                if (int.Parse(endTimeIndex) < int.Parse(startTimeIndex) || int.Parse(endTimeIndex) == int.Parse(startTimeIndex))
                {
                    TempData["error"] = "End Time must not be the same or earlier than Start Time";
                    return RedirectToAction("Index", obj);
                }

                VitasysEHR.Models.Clinic objFromDb = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == obj.Clinic.Id);

                // Update only modified fields
                if (obj.Clinic.Name != AesOperation.DecryptString(objFromDb.Name)) objFromDb.Name = AesOperation.EncryptString(obj.Clinic.Name);
                if (obj.Clinic.EmailAddress != AesOperation.DecryptString(objFromDb.EmailAddress)) objFromDb.EmailAddress = AesOperation.EncryptString(obj.Clinic.EmailAddress);
                if (obj.Clinic.Address != AesOperation.DecryptString(objFromDb.Address)) objFromDb.Address = AesOperation.EncryptString(obj.Clinic.Address);
                if (obj.Clinic.City != AesOperation.DecryptString(objFromDb.City)) objFromDb.City = AesOperation.EncryptString(obj.Clinic.City);
                if (obj.Clinic.Province != AesOperation.DecryptString(objFromDb.Province)) objFromDb.Province = AesOperation.EncryptString(obj.Clinic.Province);
                if (obj.Clinic.ZipCode != AesOperation.DecryptString(objFromDb.ZipCode)) objFromDb.ZipCode = AesOperation.EncryptString(obj.Clinic.ZipCode);
                if (obj.Clinic.OfficePhone != AesOperation.DecryptString(objFromDb.OfficePhone)) objFromDb.OfficePhone = AesOperation.EncryptString(obj.Clinic.OfficePhone);
                if (obj.Clinic.MobilePhone != AesOperation.DecryptString(objFromDb.MobilePhone)) objFromDb.MobilePhone = AesOperation.EncryptString(obj.Clinic.MobilePhone);
                if (obj.Clinic.StartTime != objFromDb.StartTime) objFromDb.StartTime = obj.Clinic.StartTime;
                if (obj.Clinic.EndTime != objFromDb.EndTime) objFromDb.EndTime = obj.Clinic.EndTime;

                _unitOfWork.Clinic.Update(objFromDb);
                _unitOfWork.Save();

                ApplicationUser user = GetCurrentUser();
                InsertLog("Update Clinic Account", user.Id, user.ClinicId, SD.AuditUpdate);

                TempData["success"] = "Clinic account details updated successfully";
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        #region 
        
        [HttpGet]
        public IActionResult LoadEditLogo()
        {
            var user = GetCurrentUser();
            var objFromDb = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);

            if (objFromDb == null) return View("Error");

            VitasysEHR.Models.Clinic clinic = new VitasysEHR.Models.Clinic()
            {
                Id = objFromDb.Id,
                LogoUrl = objFromDb.LogoUrl,
                DateAdded = objFromDb.DateAdded
            };

            InsertLog("View Edit Clinic Logo Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditClinicLogo", clinic);
        }
        
        [HttpGet]
        public IActionResult LoadEditForm()
        {
            var user = GetCurrentUser();
            var objFromDb = _unitOfWork.Clinic.GetFirstOrDefault(x => x.Id == user.ClinicId);

            ClinicVM clinic = new ClinicVM();
            
            VitasysEHR.Models.Clinic clinic1 = new VitasysEHR.Models.Clinic()
            {
                Id = objFromDb.Id,
                Name = AesOperation.DecryptString(objFromDb.Name),
                EmailAddress = AesOperation.DecryptString(objFromDb.EmailAddress),
                Address = AesOperation.DecryptString(objFromDb.Address),
                City = AesOperation.DecryptString(objFromDb.City),
                Province = AesOperation.DecryptString(objFromDb.Province),
                ZipCode = AesOperation.DecryptString(objFromDb.ZipCode),
                OfficePhone = AesOperation.DecryptString(objFromDb.OfficePhone),
                MobilePhone = AesOperation.DecryptString(objFromDb.MobilePhone),
                DateAdded = objFromDb.DateAdded,
                StartTime = objFromDb.StartTime,
                EndTime = objFromDb.EndTime,
            };
            clinic.Clinic = clinic1;
            clinic.StartTimeList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "8AM", Value = "0-8AM"},
                new SelectListItem { Text = "9AM", Value = "1-9AM"},
                new SelectListItem { Text = "10AM", Value = "2-10AM"},
                new SelectListItem { Text = "11AM", Value = "3-11AM"},
                new SelectListItem { Text = "12NN", Value = "4-12NN"},
                new SelectListItem { Text = "1PM", Value = "5-1PM"},
                new SelectListItem { Text = "2PM", Value = "6-2PM"},
                new SelectListItem { Text = "3PM", Value = "7-3PM"},
                new SelectListItem { Text = "4PM", Value = "8-4PM"},
                new SelectListItem { Text = "5PM", Value = "9-5PM"},
                new SelectListItem { Text = "6PM", Value = "10-6PM"},
                new SelectListItem { Text = "7PM", Value = "11-7PM"},
            };
            clinic.EndTimeList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "8AM", Value = "0-8AM"},
                new SelectListItem { Text = "9AM", Value = "1-9AM"},
                new SelectListItem { Text = "10AM", Value = "2-10AM"},
                new SelectListItem { Text = "11AM", Value = "3-11AM"},
                new SelectListItem { Text = "12NN", Value = "4-12NN"},
                new SelectListItem { Text = "1PM", Value = "5-1PM"},
                new SelectListItem { Text = "2PM", Value = "6-2PM"},
                new SelectListItem { Text = "3PM", Value = "7-3PM"},
                new SelectListItem { Text = "4PM", Value = "8-4PM"},
                new SelectListItem { Text = "5PM", Value = "9-5PM"},
                new SelectListItem { Text = "6PM", Value = "10-6PM"},
                new SelectListItem { Text = "7PM", Value = "11-7PM"},
            };

            

            InsertLog("View Edit Clinic Account Form", user.Id, user.ClinicId, SD.AuditView);

            return PartialView("_EditClinicAccount", clinic);
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
                return "Your next billing date is approaching! To pay your bill, click the 'Pay with GCash' button below.";
            }
            else if (dateComparisonResult == 0)
            {
                //Today
                return $"The billing date is today! You have until {gracePeriod:dd MMMM yyyy} to pay before your account reverts to the FREE version.";
            }
            else if (dateComparisonResult > 0)
            {
                //Later
                int gracePeriodComparisonResult = DateTime.Compare(dateToday, gracePeriodDate);

                if (gracePeriodComparisonResult == -1)
                {
                    //Earlier
                    return $"Your billing date has passed! You still have until {gracePeriod:dd MMMM yyyy} to pay before your account reverts to the FREE version.";
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
                    
                    return "Your account has been reverted to the FREE version. Pay your bill now to reclaim your account's previous features and functionalities.";
                }
                else if (gracePeriodComparisonResult == 0)
                {
                    //Today
                    return $"The grace period expires today ({gracePeriod:dd MMMM yyyy}). If you do not pay today, your account will revert to the FREE version tomorrow.";
                }
            }

            return null;
        }

        public bool SetIsDueForPayment(Subscription subscription)
        {
            DateTime today = DateTime.Now;

            DateTime dateToday = new(today.Year, today.Month, today.Day);
            DateTime billingDate = new(subscription.BillingDate.Year, subscription.BillingDate.Month, subscription.BillingDate.Day);

            if (billingDate.Month > today.Month) return false;

            int dateComparisonResult = DateTime.Compare(dateToday, billingDate);

            if (dateComparisonResult == -1 && today.Day - billingDate.Day >= -7)
            {
                //Earlier
                return true;
            }
            else if (dateComparisonResult == 0)
            {
                //Today
                return true;
            }
            else if (dateComparisonResult > 0)
            {
                //Later
                return true;
            }

            return false;
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
        #endregion
    }
}
