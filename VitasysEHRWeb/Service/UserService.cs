using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using VitasysEHR.Models;
using VitasysEHRWeb.Service.IService;

namespace VitasysEHRWeb.Service
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(_httpContextAccessor?.HttpContext?.User);
        }
    }
}
