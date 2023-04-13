// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Disable2faModel> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public Disable2faModel(
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Cannot disable 2FA for user as it's not currently enabled.");
            }

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "View Disable 2FA",
                ClinicId = user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                Description = SD.AuditView,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA.");
            }

            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "Disable 2FA",
                ClinicId = user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                Description = SD.AuditDisable2Fa,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
            //Audit
            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "Disable 2FA",
                DateAdded = DateTime.Now,
                UserId = await _userManager.GetUserIdAsync(user),
                Description = SD.AuditDisable2Fa

            });


            StatusMessage = "2fa has been disabled. You can reenable 2fa when you setup an authenticator app";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
