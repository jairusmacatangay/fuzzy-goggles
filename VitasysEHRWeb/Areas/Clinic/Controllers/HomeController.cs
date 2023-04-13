using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;
using VitasysEHR.Utility;

namespace VitasysEHRWeb.Controllers
{
    [Area("Clinic")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(SD.Role_Owner) || User.IsInRole(SD.Role_Dentist) || User.IsInRole(SD.Role_Assistant))
                return RedirectToAction("Index", "Patient", new { area = "Clinic" });
                
            if (User.IsInRole(SD.Role_Admin))
                return RedirectToAction("Index", "Account", new { area = "Admin" });

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsConditions()
        {
            return View();
        }

        [Route("UsersList")]
        public IActionResult UsersList()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ApplicationUser GetCurrentUser()
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim? claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string? userid = claim.Value;
            return _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == userid);
        }
    }
}