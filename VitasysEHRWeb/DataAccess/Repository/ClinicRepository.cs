using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitasysEHR.DataAccess.Repository.IRepository;
using VitasysEHR.Models;

namespace VitasysEHR.DataAccess.Repository
{
    public class ClinicRepository : Repository<Clinic>, IClinicRepository
    {
        private ApplicationDbContext _db;

        public ClinicRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Clinic obj)
        {
            var objFromDb = _db.Clinics.FirstOrDefault(u => u.Id == obj.Id);
            if(objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.Address = obj.Address;
                objFromDb.City = obj.City;
                objFromDb.Province = obj.Province;
                objFromDb.ZipCode = obj.ZipCode;
                objFromDb.OfficePhone = obj.OfficePhone;
                objFromDb.MobilePhone = obj.MobilePhone;
                objFromDb.EmailAddress = obj.EmailAddress;
                objFromDb.LastModified = DateTime.Now;
            }
        }

        public void UpdateLogo(Clinic obj)
        {
            var objFromDb = _db.Clinics.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.LastModified = DateTime.Now;
                if(!string.IsNullOrEmpty(obj.LogoUrl))
                {
                    objFromDb.LogoUrl = obj.LogoUrl;
                }
            }
        }
    }
}
