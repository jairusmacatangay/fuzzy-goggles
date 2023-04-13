// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitasysEHR.DataAccess;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IndexModel> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            ILogger<IndexModel> logger,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            
            public string? Id { get; set; }
            [Required, Display(Name = "First Name")]
            [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "First name must not contain special characters or numbers")]
            [DataType(DataType.Text)]
            [MinLength(2, ErrorMessage = "First Name must be at least 2 letters")]
            public string? FirstName { get; set; }
            [Display(Name = "Middle Name")]
            [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "Middle name must not contain special characters or numbers")]
            [DataType(DataType.Text)]
            [MinLength(2, ErrorMessage = "Middle Name must be at least 2 letters")]
            public string? MiddleName { get; set; }
            [Required, Display(Name = "Last Name")]
            [RegularExpression("[a-zA-Z]+ ?[a-zA-Z]*", ErrorMessage = "Last name must not contain special characters or numbers")]
            [DataType(DataType.Text)]
            [MinLength(2, ErrorMessage = "Last Name must be at least 2 letters")]
            public string? LastName { get; set; }
            [Required]
            public string? Gender { get; set; }
            [Required, Display(Name = "Date of Birth")]
            public string? DOB { get; set; }
            [Required]
            [MinLength(2, ErrorMessage = "Address requires at least 2 letters")]
            public string? Address { get; set; }
            [Required]
            [MinLength(2, ErrorMessage = "City requires at least 2 letters")]
            public string? City { get; set; }
            [Required]
            [MinLength(2, ErrorMessage = "Province requires at least 2 letters")]
            public string? Province { get; set; }
            [Display(Name = "Zip Code")]
            [MaxLength(4,ErrorMessage = "Zip code must be 4 digits")]
            [MinLength(4,ErrorMessage = "Zip code must be 4 digits")]
            [RegularExpression(@"\d{4,4}",ErrorMessage ="Zip code must be a number")]
            public string? ZipCode { get; set; }

            [Display(Name = "Mobile number")]
            [Required]
            [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Please follow the format: 09XXXXXXXXX or +639XXXXXXXXX")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Profile Picture")]
            public string ProfPicUrl {get; set;}
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = AesOperation.DecryptString(user.FirstName),
                MiddleName = AesOperation.DecryptString(user.MiddleName),
                LastName = AesOperation.DecryptString(user.LastName),
                Gender = AesOperation.DecryptString(user.Gender),
                DOB = AesOperation.DecryptString(user.DOB),
                Address = AesOperation.DecryptString(user.Address),
                City = AesOperation.DecryptString(user.City),
                Province = AesOperation.DecryptString(user.Province),
                ZipCode = AesOperation.DecryptString(user.ZipCode),
                PhoneNumber = AesOperation.DecryptString(phoneNumber),
                ProfPicUrl = user.ProfPicUrl
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "View Profile",
                ClinicId = user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                Description = SD.AuditView,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);
            string wwwRootPath = _hostEnvironment.WebRootPath;
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.FirstName = AesOperation.EncryptString(Input.FirstName);
            user.MiddleName = AesOperation.EncryptString(Input.MiddleName);
            user.LastName = AesOperation.EncryptString(Input.LastName);
            user.Gender = AesOperation.EncryptString(Input.Gender);
            user.DOB = AesOperation.EncryptString(Input.DOB);
            user.Address = AesOperation.EncryptString(Input.Address);
            user.City = AesOperation.EncryptString(Input.City);
            user.Province = AesOperation.EncryptString(Input.Province);
            user.ZipCode = AesOperation.EncryptString(Input.ZipCode);
            user.PhoneNumber = AesOperation.EncryptString(Input.PhoneNumber);
            user.LastModified = DateTime.Now;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(wwwRootPath, @"img\user-profile-pics");
                var extension = Path.GetExtension(file.FileName);
                if (!imageExtensions.Contains(extension))
                {
                    TempData["error"] = "The file type is not supported in this field. Please upload images only.";
                    return RedirectToPage();
                }
                if (Input.ProfPicUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, Input.ProfPicUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }

                Input.ProfPicUrl = @"\img\user-profile-pics\" + fileName + extension;
                user.ProfPicUrl = AesOperation.EncryptString(Input.ProfPicUrl);
            }

            var result = await _userManager.UpdateAsync(user);

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "Update User Profile",
                DateAdded = DateTime.Now,
                ClinicId = user.ClinicId,
                UserId = user.Id,
                Description = SD.AuditUpdate,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });

            _unitOfWork.Save();

            if (result.Succeeded)
            {
                _logger.LogInformation("User updated account information");
            }

            if (user.ClinicId == null)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["success"] = "Your profile has been updated";
                return RedirectToAction("CreateClinic", "Account", new { area = "Clinic"});
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";

            return RedirectToPage();
        }
    }
}
