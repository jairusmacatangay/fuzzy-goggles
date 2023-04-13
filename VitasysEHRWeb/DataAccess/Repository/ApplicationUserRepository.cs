using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

         public void Update(ApplicationUser obj)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.FirstName = obj.FirstName;
                objFromDb.MiddleName = obj.MiddleName;
                objFromDb.LastName = obj.LastName;
                objFromDb.DOB = obj.DOB;
                objFromDb.Email = obj.Email;
                objFromDb.PhoneNumber = obj.PhoneNumber;
                objFromDb.Gender = obj.Gender;
                objFromDb.Address = obj.Address;
                objFromDb.City = obj.City;
                objFromDb.Province = obj.Province;
                objFromDb.ZipCode = obj.ZipCode;
                objFromDb.LastModified = DateTime.Now;
                objFromDb.Clinic = obj.Clinic;
                objFromDb.IsArchived = obj.IsArchived;
                if (obj.ProfPicUrl != null)
                {
                    objFromDb.ProfPicUrl = obj.ProfPicUrl;
                }
                if (objFromDb.EmailConfirmed == false)
                {
                    objFromDb.EmailConfirmed = obj.EmailConfirmed;
                }
            }

        }

    }
}
