// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LogoutModel> logger, 
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userId);
            HttpContext.Session.Remove(SD.SessionKeyPatientId);
            HttpContext.Session.Remove(SD.SessionKeySubscriptionType);
            HttpContext.Session.Remove(SD.SessionKeySubscriptionIsLockout);
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out.");
 
            _unitOfWork.AuditLog.Add(new AuditLog
            {
                ActivityType = "Logout",
                ClinicId = user.ClinicId,
                DateAdded = DateTime.Now,
                UserId = user.Id,
                Description = SD.AuditLogout,
                Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            _unitOfWork.Save();

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
