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
    public class PermitReverifyModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProfileModel> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public PermitReverifyModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            ILogger<ProfileModel> logger,
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

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            public string? Id { get; set; }

            public string Permit { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            
            Email = email;

            Input = new InputModel { Permit = user.Permit };
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

        public async Task<IActionResult> OnPostAsync(IFormFile permit)
        {
            var user = await _userManager.GetUserAsync(User);
            string wwwRootPath = _hostEnvironment.WebRootPath;

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.LastModified = DateTime.Now;
            user.AdminVerified = "Pending";

            if (permit != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(wwwRootPath, @"img\permits");
                var extension = Path.GetExtension(permit.FileName);

                if (Input.Permit != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, Input.Permit.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                {
                    permit.CopyTo(fileStreams);
                }

                Input.Permit = @"\img\permits\" + fileName + extension;
                user.Permit = AesOperation.EncryptString(Input.Permit);
            }
            var result = await _userManager.UpdateAsync(user);

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "Update User Permit",
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
                TempData["StatusMessage"] = "Please complete your clinic information.";
                return RedirectToAction("CreateClinic", "Account", new { area = "Clinic" });
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";

            return RedirectToAction("Index", "Account", new { area = "Clinic" });
        }
    }
}
