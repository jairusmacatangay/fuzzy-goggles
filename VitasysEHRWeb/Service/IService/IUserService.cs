using VitasysEHR.Models;

namespace VitasysEHRWeb.Service.IService
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUser();
    }
}
