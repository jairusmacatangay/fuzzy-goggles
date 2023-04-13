// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using VitasysEHR.DataAccess;
using VitasysEHR.Models;
using VitasysEHR.Utility;
using VitasysEHR.DataAccess.Repository.IRepository;

namespace VitasysEHRWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }


            returnUrl ??= Url.Content("~/");
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl);
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Clinic/Patient");
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    int? id = HttpContext.Session.GetInt32(SD.SessionKeyPatientId);
                    if (id != null)
                    {
                        HttpContext.Session.Remove(SD.SessionKeyPatientId);
                    }
                    int? userId = HttpContext.Session.GetInt32(SD.SessionKeyUserId);

                    if (user == null)
                    {
                        _unitOfWork.AuditLog.Add(new AuditLog
                        {
                            ActivityType = "Login",
                            DateAdded = DateTime.Now,
                            Description = "Non-existing user attempted login",
                            Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        });
                        _unitOfWork.Save();

                        ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                        return Page();
                    }

                    if (user.IsArchived)
                    {
                        _unitOfWork.AuditLog.Add(new AuditLog
                        {
                            ActivityType = "Login",
                            DateAdded = DateTime.Now,
                            Description = "Archived User attempted Login",
                            Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        });
                        _unitOfWork.Save();

                        ModelState.AddModelError(string.Empty, "This account has been archived.");
                        return Page();
                    }

                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    var confirmed = await _userManager.IsEmailConfirmedAsync(user);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.ElementAt(0) == "Admin")
                    {
                        return RedirectToAction("Index", "Account", new { area = "Admin" });
                    }

                    if (confirmed && result.Succeeded && !user.IsArchived)
                    {
                        _logger.LogInformation("User logged in.");

                        _unitOfWork.AuditLog.Add(new AuditLog
                        {
                            ActivityType = "Login",
                            DateAdded = DateTime.Now,
                            ClinicId = user.ClinicId,
                            UserId = user.Id,
                            Description = SD.AuditLogin,
                            Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        });
                        _unitOfWork.Save();

                        if (user.FirstName == null)
                        {
                            TempData["StatusMessage"] = "Please fill in your profile information.";
                            return RedirectToPage("/Account/Manage/Profile", new { area = "Identity" });
                        }
                        else if (user.ClinicId == null)
                        {
                            return RedirectToAction("CreateClinic", "Account", new { area = "Clinic" });
                        }

                        var subscription = _unitOfWork.Subscription.GetFirstOrDefault(x => x.ClinicId == user.ClinicId);

                        if (User.IsInRole(SD.Role_Owner) && subscription == null)
                        {
                            TempData["StatusMessage"] = "Please choose your subscription plan.";
                            return RedirectToAction("Index", "Subscription", new { area = "Clinic" });
                        }

                        HttpContext.Session.SetString(SD.SessionKeySubscriptionType, subscription.Type);
                        HttpContext.Session.SetString(SD.SessionKeySubscriptionIsLockout, subscription.IsLockout ? "true" : "false");
                                            

                        return LocalRedirect(returnUrl);
                    }
                    else if (!confirmed)
                    {
                        return RedirectToPage("/Account/ConfirmError", new { area = "Identity" });
                    }

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }

                    ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                    return Page();
                }

                return Page();
            }
            catch (ArgumentNullException ex)
            {
                _unitOfWork.AuditLog.Add(new AuditLog
                {
                    ActivityType = "Login",
                    DateAdded = DateTime.Now,
                    Description = "Non-existing user attempted login",
                    Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
                });
                _unitOfWork.Save();

                Console.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                return Page();
            }
            catch (NullReferenceException ex)
            {
                _unitOfWork.AuditLog.Add(new AuditLog
                {
                    ActivityType = "Login",
                    DateAdded = DateTime.Now,
                    Description = "Non-existing user attempted login",
                    Device = HttpContext.Connection.RemoteIpAddress?.ToString(),
                });
                _unitOfWork.Save();

                Console.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, "Invalid E-mail or password.");
                return Page();
            }
        }
    }
}
